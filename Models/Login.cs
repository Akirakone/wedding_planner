using System;
using System.ComponentModel.DataAnnotations;



namespace wedding_planner.Models
{
    public class Login
    {

        [Required]
        [EmailAddress]
        public string lEmail { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string lPassword { get; set; }
    }
}