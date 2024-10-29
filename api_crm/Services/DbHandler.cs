using api_crm.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace api_crm.Services
{
    public class DbHandler(IConfiguration configuration, ILogger<DbHandler> logger)
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<DbHandler> _logger = logger;
        private readonly bool _isTestMode = configuration?.GetValue<bool>("Environment:IsTestMode") ?? true;

        private string GetErpConnectionString()
        {
            return _isTestMode
                ? _configuration.GetConnectionString("ErpTest") ?? string.Empty
                : _configuration.GetConnectionString("ErpProd") ?? string.Empty;
        }

        #region GetData

        public async Task<decimal> GetCustomerBalanceSingleAsync(string customerCode)
        {
            string query = @"SELECT
                            ISNULL(SUM(PostedAmount * (CASE Direction WHEN 0 THEN 1 ELSE -1 END)),0) AS Balance
                        FROM FinanceTransactions
                        WHERE Client = '00'
                            AND Company = 'CMP1'
                            AND IsDeleted = 0
                            AND AccountType IN ('C','V')
                            AND GlAccount LIKE '211%'
                            AND PostingDate <= CURRENT_TIMESTAMP
                            AND AccountCode = @customerCode";
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("ErpProd"));
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@customerCode", customerCode);

                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    return 0;
                }

                return Convert.ToDecimal(result);
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "DbHandler.GetCustomerBalanceSingleAsync", $"An error occurred while getting customer balance", ex);

                throw;
            }
        }

        public async Task<object> GetDocumentDataAsync(string documentTypeNumber, bool useTestEnvironment)
        {
            var result = new
            {
                Header = new Dictionary<string, object>(),
                Items = new List<Dictionary<string, object>>()
            };


            try
            {
                string queryHead = @"SELECT * FROM SalesHead
                                        WHERE Client = '00'
                                            AND Company = 'CMP1'
                                            AND IsDeleted = 0
                                            AND DocumentType + DocumentNumber = @documentTypeNumber";

                using var connection = new SqlConnection(_configuration.GetConnectionString(useTestEnvironment ? "ErpTest" : "ErpProd"));
                using var commandHead = new SqlCommand(queryHead, connection);
                commandHead.Parameters.AddWithValue("@documentTypeNumber", documentTypeNumber);

                await connection.OpenAsync();
                using (var readerHead = await commandHead.ExecuteReaderAsync())
                {

                    if (await readerHead.ReadAsync())
                    {
                        for (int i = 0; i < readerHead.FieldCount; i++)
                        {
                            result.Header[readerHead.GetName(i)] = readerHead.GetValue(i);
                        }
                    }
                }

                string queryItem = @"SELECT * FROM SalesItem
                                        WHERE Client = '00'
                                            AND Company = 'CMP1'
                                            AND IsDeleted = 0
                                            AND DocumentType + DocumentNumber = @documentTypeNumber";

                using var commandItem = new SqlCommand(queryItem, connection);
                commandItem.Parameters.AddWithValue("@documentTypeNumber", documentTypeNumber);

                using (var readerItem = await commandItem.ExecuteReaderAsync())
                {
                    while (await readerItem.ReadAsync())
                    {
                        var item = new Dictionary<string, object>();
                        for (int i = 0; i < readerItem.FieldCount; i++)
                        {
                            item[readerItem.GetName(i)] = readerItem.GetValue(i);
                        }
                        result.Items.Add(item);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "DbHandler.GetDocumentDataAsync", $"An error occurred while retrieving document data", ex);

                throw;
            }
        }

        public async Task<List<AgingReport>> GetAllAgingReportsAsync()
        {
            var AllAgingReports = new List<AgingReport>();

            string query = @"SELECT
                                A.account AS CustomerCode,
                                M.definition AS CustomerName,
                                C.back_manager AS Manager,
                                A.current_balance AS CurrentBalance,
                                A.days_0_30 AS Days0To30,
                                A.days_31_60 AS Days31To60,
                                A.days_61_90 AS Days61To90,
                                A.days_91_120 AS Days91To120,
                                A.days_121_150 AS Days121To150,
                                A.days_151_180 AS Days151To180,
                                A.days_181_210 AS Days181To210,
                                A.days_211_240 AS Days211To240,
                                A.days_241_270 AS Days241To270,
                                A.days_271_300 AS Days271To300,
                                A.days_301_330 AS Days301To330,
                                A.days_331_360 AS Days331To360,
                                A.days_360_plus AS Days360Plus
                            FROM accounts_aging A
                            LEFT JOIN master_account M
                                ON (A.account = M.account)
                            LEFT JOIN master_customer C
                                ON (A.account = C.code)
                            ORDER BY A.account";

            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DWH"));
                using var command = new SqlCommand(query, connection);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var agingReport = new AgingReport
                    {
                        CustomerCode = reader["CustomerCode"].ToString() ?? string.Empty,
                        CustomerName = reader["CustomerName"].ToString() ?? string.Empty,
                        Manager = reader["Manager"].ToString() ?? string.Empty,
                        CurrentBalance = Convert.ToDecimal(reader["CurrentBalance"]),
                        Days0To30 = Convert.ToDecimal(reader["Days0To30"]),
                        Days31To60 = Convert.ToDecimal(reader["Days31To60"]),
                        Days61To90 = Convert.ToDecimal(reader["Days61To90"]),
                        Days91To120 = Convert.ToDecimal(reader["Days91To120"]),
                        Days121To150 = Convert.ToDecimal(reader["Days121To150"]),
                        Days151To180 = Convert.ToDecimal(reader["Days151To180"]),
                        Days181To210 = Convert.ToDecimal(reader["Days181To210"]),
                        Days211To240 = Convert.ToDecimal(reader["Days211To240"]),
                        Days241To270 = Convert.ToDecimal(reader["Days241To270"]),
                        Days271To300 = Convert.ToDecimal(reader["Days271To300"]),
                        Days301To330 = Convert.ToDecimal(reader["Days301To330"]),
                        Days331To360 = Convert.ToDecimal(reader["Days331To360"]),
                        Days360Plus = Convert.ToDecimal(reader["Days360Plus"])
                    };

                    AllAgingReports.Add(agingReport);
                }
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "DbHandler.GetAllAgingReportsAsync", $"An error occurred while getting all aging reports", ex);
                throw;
            }

            return AllAgingReports;
        }

        #endregion

        #region CheckExistance

        public async Task<bool> ManagerExistsAsync(string managerCode)
        {
            string query = "SELECT COUNT(*) FROM ManagerList WHERE Client = '00' AND Company = 'CMP1' AND ManagerCode = @ManagerCode";

            try
            {
                using var connection = new SqlConnection(GetErpConnectionString());
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ManagerCode", managerCode);

                await connection.OpenAsync();

                int count = Convert.ToInt32(await command.ExecuteScalarAsync());

                return count > 0;
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "DbHandler.ManagerExistsAsync", $"An error occurred while checking if manager exists", ex);
                throw;
            }
        }

        public async Task<bool> SegmentExistsAsync(string segmentCode)
        {
            string query = "SELECT COUNT(*) FROM SegmentList WHERE Client = '00' AND Company = 'CMP1' AND SegmentCode = @SegmentCode";

            try
            {
                using var connection = new SqlConnection(GetErpConnectionString());
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SegmentCode", segmentCode);

                await connection.OpenAsync();

                int count = Convert.ToInt32(await command.ExecuteScalarAsync());

                return count > 0;
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "DbHandler.SegmentExistsAsync", $"An error occurred while checking if segment exists", ex);
                throw;
            }
        }

        public async Task<bool> CustomerExistsAsync(string customerCode)
        {
            string query = "SELECT COUNT(*) FROM Customer WHERE Client = '00' AND Company = 'CMP1' AND CustomerCode = @CustomerCode AND IsDeleted = 0";

            try
            {
                using var connection = new SqlConnection(GetErpConnectionString());
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CustomerCode", customerCode);

                await connection.OpenAsync();

                int count = Convert.ToInt32(await command.ExecuteScalarAsync());

                return count > 0;
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "DbHandler.CustomerExistsAsync", $"An error occurred while checking if customer exists", ex);
                throw;
            }
        }

        #endregion

        #region ModifyData

        public async Task<string> CreateCustomerAsync(CreateCustomerRequest customerRequest)
        {
            try
            {
                using var connection = new SqlConnection(GetErpConnectionString());
                using var command = new SqlCommand("CreateCustomerERP", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@CustomerName", customerRequest.Name);
                command.Parameters.AddWithValue("@CreatedBy", customerRequest.CreatedBy);
                command.Parameters.AddWithValue("@Manager", customerRequest.Manager);
                command.Parameters.AddWithValue("@Segment", customerRequest.Segment);
                command.Parameters.AddWithValue("@MobileNumber", customerRequest.MobileNumber);
                command.Parameters.AddWithValue("@CustomerGroup", customerRequest.IsCompany == true ? "03" : "02");

                var outputParam = new SqlParameter("@CreatedCustomer", SqlDbType.NVarChar, 10)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputParam);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                Log(LogLevel.Information, "DbHandler.CustomerCreateAsync", $"Successfully created customer {outputParam.Value?.ToString() ?? string.Empty}");

                return outputParam.Value?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "DbHandler.CustomerCreateAsync", $"An error occurred while creating the customer", ex);

                throw;
            }
        }

        public async Task UpdateCustomerAsync(UpdateCustomerRequest customerRequest)
        {
            string queryCustomer = @"UPDATE Customer
                        SET CustomerName = @CustomerName,
                            SALDEPT = @Manager,
                            BRANCH = @Segment,
                            TELNUM = @MobileNumber,
                            ChangedBy = @ChangedBy,
                            ChangedAt = CURRENT_TIMESTAMP
                        WHERE CustomerCode = @CustomerCode";

            string queryAccount = @"UPDATE AccountNames
                        SET ShortDescription = @CustomerName,
                            ChangedBy = @ChangedBy,
                            ChangedAt = CURRENT_TIMESTAMP
                        WHERE AccountCode = @CustomerCode";

            try
            {
                using var connection = new SqlConnection(GetErpConnectionString());
                await connection.OpenAsync();

                // Customer
                using var commandCustomer = new SqlCommand(queryCustomer, connection);
                commandCustomer.Parameters.AddWithValue("@CustomerCode", customerRequest.CustomerCode);
                commandCustomer.Parameters.AddWithValue("@CustomerName", customerRequest.Name);
                commandCustomer.Parameters.AddWithValue("@Manager", customerRequest.Manager);
                commandCustomer.Parameters.AddWithValue("@Segment", customerRequest.Segment);
                commandCustomer.Parameters.AddWithValue("@MobileNumber", customerRequest.MobileNumber);
                commandCustomer.Parameters.AddWithValue("@ChangedBy", customerRequest.ChangedBy);

                int affectedRowsCustomer = await commandCustomer.ExecuteNonQueryAsync();

                if (affectedRowsCustomer == 0)
                {
                    string errorMessage = $"No rows affected after updating customer {customerRequest.CustomerCode}";
                    Log(LogLevel.Error, "DbHandler.UpdateCustomerAsync", errorMessage);
                    throw new Exception(errorMessage);
                }

                // Account
                using var commandAccount = new SqlCommand(queryAccount, connection);
                commandAccount.Parameters.AddWithValue("@CustomerCode", customerRequest.CustomerCode);
                commandAccount.Parameters.AddWithValue("@CustomerName", customerRequest.Name);
                commandAccount.Parameters.AddWithValue("@ChangedBy", customerRequest.ChangedBy);

                int affectedRowsAccount = await commandAccount.ExecuteNonQueryAsync();

                if (affectedRowsAccount == 0)
                {
                    string errorMessage = $"No rows affected after updating account {customerRequest.CustomerCode}";
                    Log(LogLevel.Error, "DbHandler.UpdateCustomerAsync", errorMessage);
                    throw new Exception(errorMessage);
                }

                Log(LogLevel.Information, "DbHandler.UpdateCustomerAsync", $"Successfully updated customer {customerRequest.CustomerCode}");
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "DbHandler.UpdateCustomerAsync", $"An error occurred while updating the customer", ex);
                throw;
            }
        }

        #endregion

        #region Logging

        public void Log(LogLevel logLevel, string loggerName, string message, Exception? ex = null)
        {

            var appLog = new AppLog(
                appName: _configuration.GetSection("AppData:Name").Value ?? "NoConfiguredAppName",
                logLevel: logLevel.ToString(),
                logger: loggerName,
                message: message,
                exception: ex?.Message ?? string.Empty,
                stackTrace: ex?.StackTrace ?? string.Empty,
                machineName: Environment.MachineName,
                requestId: 0,
                createdAt: DateTime.Now
            );

            InsertAppLog(appLog);
        }

        public void InsertAppLog(AppLog log)
        {
            string insertQuery = @"
            INSERT INTO AppLog (AppName, LogLevel, Logger, Message, Exception, StackTrace, MachineName, RequestId, CreatedAt)
            VALUES (@AppName, @LogLevel, @Logger, @Message, @Exception, @StackTrace, @MachineName, @RequestId, @CreatedAt)"
            ;

            try
            {
                using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DEV"));
                using SqlCommand command = new SqlCommand(insertQuery, connection);

                command.Parameters.AddWithValue("@AppName", log.AppName);
                command.Parameters.AddWithValue("@LogLevel", log.LogLevel);
                command.Parameters.AddWithValue("@Logger", log.Logger);
                command.Parameters.AddWithValue("@Message", log.Message);
                command.Parameters.AddWithValue("@Exception", log.Exception);
                command.Parameters.AddWithValue("@StackTrace", log.StackTrace);
                command.Parameters.AddWithValue("@MachineName", log.MachineName);
                command.Parameters.AddWithValue("@RequestId", log.RequestId);
                command.Parameters.AddWithValue("@CreatedAt", log.CreatedAt);

                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inserting the log");
            }
        }

        #endregion
    }
}
