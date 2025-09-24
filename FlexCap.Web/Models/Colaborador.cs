﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlexCap.Web.Models
{
    public class Colaborador
    {
        [Key]
        public int Id { get; set; }
        public string FotoUrl { get; set; }
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string Cargo { get; set; }
        public string Setor { get; set; }
        public string Status { get; set; }
        public string Pais { get; set; }
        [Column ("Team")]
        public string Time { get; set; }

    }
}
