using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_2_BE.Models
{
    [Table("ChatBotAI")]
    public class ChatBotAIModel
    {
        [Key]
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("AccountId")]
        public Guid AccountId { get; set; }

        [Column("Message")]
        public string Message { get; set; } = string.Empty;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }
        public AccountsModel Account { get; set; }

    }
}
