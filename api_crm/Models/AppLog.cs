using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace api_crm.Models
{
    [Table("AppLog")]
    public class AppLog(
        string appName,
        string logLevel,
        string logger,
        string message,
        string exception,
        string stackTrace,
        string machineName,
        long requestId,
        DateTime createdAt)
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public string AppName { get; init; } = appName;

        [Required]
        public string LogLevel { get; init; } = logLevel;

        [Required]
        public string Logger { get; init; } = logger;

        [Required]
        public string Message { get; init; } = message;

        [Required]
        public string Exception { get; init; } = exception;

        [Required]
        public string StackTrace { get; init; } = stackTrace;

        [Required]
        public string MachineName { get; init; } = machineName;

        [Required]
        public long RequestId { get; init; } = requestId;

        [Required]
        public DateTime CreatedAt { get; init; } = createdAt;
    }
}
