using Cpf_Validator.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using ClosedXML.Excel;
using cpf_Validator_Colunas;
using System.Threading.Tasks;


namespace CpfValidator
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Montagem da lista de pessoas!");


            try
            {
                string filePath = @"C:\Users\Daniel Saes\Pagare\Id-wall-Assincrono\src\IN\Teste_ID-Wall.csv";

                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    var cpfs = new List<Person>();

                    Console.WriteLine("");
                    Console.WriteLine("");
                    Console.WriteLine("Capturando e corrigindo cpf's enviados...");

                    while ((line = reader.ReadLine()) != null)
                    {
                        Person person = line;

                        if (person.CPF.Length < 11)
                        {
                            do
                            {
                                person.CPF = "0" + person.CPF;
                            } while (person.CPF.Length != 11);
                        }

                        cpfs.Add(person);
                    }


                    Console.WriteLine();

                    string receivedToken = await GerarTokenAsync();

                    Console.WriteLine("");
                    Console.WriteLine("");
                    Console.WriteLine("iniciando verificação de " + cpfs.Count.ToString() + " cpf's");
                    Console.WriteLine("");


                    List<PersonOk> CpfsValidados = await ValidaCPFAsync(cpfs, cpfs.Count, "Bearer " + receivedToken);

                    Console.WriteLine("");
                    Console.WriteLine("Iniciando montagem de Documento excel...");

                    MontaDocumento(CpfsValidados);

                    Console.WriteLine(receivedToken);

                    Console.ReadLine();

                }
            }
            catch (Exception)
            {
                Console.WriteLine("deu ruim");
            }


        }
        private static async Task<string> GerarTokenAsync()
        {
            using (var httpClient = new HttpClient())
            {
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://gateway.apiserpro.serpro.gov.br/token?grant_type=client_credentials"))
                {
                    requestMessage.Headers.Add("Authorization", "API_AUTHORIZATION");
                    requestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");


                    var response = await httpClient.SendAsync(requestMessage);
                    var token = await response.Content.ReadAsStringAsync();

                    dynamic retorno = JsonConvert.DeserializeObject(token);

                    string receivedToken;

                    receivedToken = retorno.access_token;

                    return receivedToken;
                }
            }
        }

        public static async Task<List<PersonOk>> ValidaCPFAsync(List<Person> persons, int total, string receivedToken)
        {
            string urlSerpro = "https://gateway.apiserpro.serpro.gov.br/consulta-cpf-df/v1/cpf/";
            string authorization = "Bearer " + receivedToken;

            int i = 0;
            var listAnalisados = new List<PersonOk>();

            try
            {
                using (var cliente = new HttpClient())
                {
                    foreach (Person person in persons)
                    {
                        cliente.DefaultRequestHeaders.Clear();
                        cliente.DefaultRequestHeaders.Add("Authorization", authorization);

                        var resposta = await cliente.GetAsync(urlSerpro + person.CPF);

                        var response = resposta;

                        if (response.IsSuccessStatusCode)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            dynamic retorno = JsonConvert.DeserializeObject(result);

                            var analisado = new PersonOk();
                            analisado.cpf = retorno.ni;
                            analisado.nome = retorno.nome;
                            analisado.dataNascimento = retorno.nascimento;
                            analisado.situacaoCpf = retorno.situacao.descricao;

                            listAnalisados.Add(analisado);
                        }
                        else
                        {
                            Console.WriteLine("Erro ao consultar CPF: " + person.CPF + " - Status Code: " + response.StatusCode);
                            var analisado = new PersonOk();
                            analisado.cpf = person.CPF;
                            analisado.nome = person.NOME;
                            analisado.dataNascimento = person.NASC;
                            analisado.situacaoCpf = "CPF improcessavel";

                            listAnalisados.Add(analisado);
                        }

                        i++;
                        Console.WriteLine("Cpf's analisados: " + i + "/" + total);
                    }

                    Console.WriteLine("");
                    Console.WriteLine(total + " cpf's analisados com sucesso");

                    return listAnalisados;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro: " + ex.Message);
                Console.WriteLine("devolvendo " + listAnalisados.Count() + "CPF's Analisados");
                return listAnalisados;
            }
        }



        public static void MontaDocumento(List<PersonOk> listaValidados)
        {
            try
            {
                //Criação do Workbook
                var wb = new XLWorkbook();

                //Criação da planilha
                var ws = wb.Worksheets.Add("TODOS");
                var wsRegular = wb.Worksheets.Add("REGULARES");
                var wsOutras = wb.Worksheets.Add("IRREGULARES");

                //Cabecalho da planilha
                ws.Cell("A1").Value = "CPF";
                ws.Cell("B1").Value = "NOME";
                ws.Cell("C1").Value = "DATA DE NASCIMENTO";
                ws.Cell("D1").Value = "SITAÇÃO CPF";

                wsRegular.Cell("A1").Value = "CPF";
                wsRegular.Cell("B1").Value = "NOME";
                wsRegular.Cell("C1").Value = "DATA DE NASCIMENTO";
                wsRegular.Cell("D1").Value = "SITAÇÃO CPF";

                wsOutras.Cell("A1").Value = "CPF";
                wsOutras.Cell("B1").Value = "NOME";
                wsOutras.Cell("C1").Value = "DATA DE NASCIMENTO";
                wsOutras.Cell("D1").Value = "SITAÇÃO CPF";

                //Corpo do relatório 
                var linha = 2;
                var linhaRegular = 2;
                var linhaOutra = 2;

                foreach (PersonOk pessoa in listaValidados)
                {

                    ws.Cell("A" + linha.ToString()).Value = pessoa.cpf;
                    ws.Cell("B" + linha.ToString()).Value = pessoa.nome;
                    ws.Cell("C" + linha.ToString()).Value = pessoa.dataNascimento;
                    ws.Cell("D" + linha.ToString()).Value = pessoa.situacaoCpf;

                    linha++;

                    if (pessoa.situacaoCpf == "Regular")
                    {

                        wsRegular.Cell("A" + linhaRegular.ToString()).Value = pessoa.cpf;
                        wsRegular.Cell("B" + linhaRegular.ToString()).Value = pessoa.nome;
                        wsRegular.Cell("C" + linhaRegular.ToString()).Value = pessoa.dataNascimento;
                        wsRegular.Cell("D" + linhaRegular.ToString()).Value = pessoa.situacaoCpf;

                        linhaRegular++;
                    }
                    else

                    {
                        wsOutras.Cell("A" + linhaOutra.ToString()).Value = pessoa.cpf;
                        wsOutras.Cell("B" + linhaOutra.ToString()).Value = pessoa.nome;
                        wsOutras.Cell("C" + linhaOutra.ToString()).Value = pessoa.dataNascimento;
                        wsOutras.Cell("D" + linhaOutra.ToString()).Value = pessoa.situacaoCpf;

                        linhaOutra++;
                    }
                }

                //Salva planinlha
                wb.SaveAs(@"C:\Users\Daniel Saes\Pagare\cpf_Validator_Colunas_Serpro_Assincrono\src\OUT\LISTA LOTE 1 (0-199999) - ID WALL TESTE - 55154CPF.xlsx");

                //Libera cache
                ws.Clear();
                wb.Dispose();
                Console.WriteLine("Planilha criada com sucesso!");

            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}


