using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IdeaAPI12_08_2025.Models
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string Msg { get; set; }

        [JsonPropertyName("id")]
        public string? ideaId { get; set; }
    }
}
