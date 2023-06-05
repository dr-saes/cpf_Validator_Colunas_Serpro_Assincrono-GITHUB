
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace Cpf_Validator.Models
{
    public class Person
    {
        public string CPF { get; set; }
        public string NOME { get; set; }
        public string NASC { get; set; }

        public static implicit operator Person(string line)
        {
            var data = line.Split(separator: ";");

            Person person = new Person();
            person.CPF = data[0];
            person.NOME = data[1];
            person.NASC = data[2];

            return person;
        }

    }
}

