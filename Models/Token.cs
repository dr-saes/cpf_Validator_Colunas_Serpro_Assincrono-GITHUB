using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cpf_Validator_Colunas

{
    public class Token
    {

        public string access_token { get; set; }
        public string scope { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }


}
