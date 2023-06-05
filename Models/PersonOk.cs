using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;



namespace Cpf_Validator.Models
{
    public class PersonOk
    {
        public string cpf { get; set; }
        public string nome { get; set; }
        public string dataNascimento { get; set; }
        public string situacaoCpf { get; set; }



    }
}