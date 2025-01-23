using GerenciadorDeTarefas.Models;
using Newtonsoft.Json;

class GerenciarTarefas
{ 
    static void Main()
    {
        Console.WriteLine("===== Sistema de Gerenciamento de Tarefas ===== \r\n");

        Console.WriteLine("1. Adicionar nova tarefa" +
            "\r\n2. Listar todas as tarefas" +
            "\r\n" +
            "3. Concluir uma tarefa" +
            "\r\n" +
            "4. Remover uma tarefa\r\n" +
            "5. Sair\r\n");

        var resposta = Convert.ToInt32(Console.ReadLine());

        Iniciar(resposta);
    }

    static void Iniciar(int id)
    {
        List<Tarefa> tarefas = new List<Tarefa>();
        string resposta = "";
        bool result = false;

        switch (id)
        {
            //Tratativa de valor sem função;
            case < 1:
                Console.WriteLine("Nenhuma funcionalidade encontrada para a opção selecionada. Por favor selecione uma opção válida");
                Main();
                break;
            //Adiciona uma nova tarefa;
            case 1:
                Console.WriteLine("Digite a descrição da tarefa!");
                var tarefa = Console.ReadLine();
                AdicionarTarefa(tarefa);

                Console.WriteLine("");
                Console.WriteLine("Tarefa adicionada!");
                Console.WriteLine("");

                Task.Delay(3000);
                Main();
                break;
            //Lista todas as tarefas;
            case 2:
                tarefas = ListarTarefas();
                if(tarefas.Count == 0)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Nenhuma tarefa encontrada\r\n");
                    Main();
                }

                Console.WriteLine("");
                Console.WriteLine("Essas são as tarefas encontradas: ");
                foreach (var item in tarefas)
                {
                    Console.WriteLine($"[{item.Id}] - {item.Descricao} - {(item.Concluida == false ? "Pendente" : "Concluída \r")}");
                }

                Console.WriteLine("\n");
                Task.Delay(3000);
                Main();
                break;
            //Marca a tarefa como concluída;
            case 3:
                tarefas = ListarTarefas();

                if(tarefas.Count == 0)
                {
                    Console.WriteLine("Nenhuma tarefa encontrada.  Gostaria de tentar novamente?");
                    if (resposta == "sim" || resposta == "si" || resposta == "s") Iniciar(2);
                    else { Main(); }
                }

                Console.WriteLine("Essas são as tarefas encontradas:");
                foreach (var item in tarefas)
                {
                    Console.WriteLine($"[{item.Id}] - {item.Descricao} - {(item.Concluida == false ? "Pendente" : "Concluída")}");
                }

                Console.Write("Por favor digite o número da tarefa que quer concluir: ");

                int tarefaAConcluir = Convert.ToInt32(Console.ReadLine());

                result = ConcluirTarefa(tarefaAConcluir);

                if (result == false) Console.WriteLine("Nenhuma tarefa encontrada com o número informado. Gostaria de tentar novamente?");

                resposta = Console.ReadLine().ToLower();

                if (resposta == "sim" || resposta == "si" || resposta == "s") Iniciar(3);
                else { Main(); }

                break;
            //Remove a tarefa do arquivo. É realizada a remoção permanente (exclusão) daquela tarefa do arquivo;
            case 4:
                tarefas = ListarTarefas();
                if(tarefas.Count == 0)
                {
                    Console.WriteLine("Nenhuma tarefa encontrada.  Gostaria de tentar novamente?");
                    if (resposta == "sim" || resposta == "si" || resposta == "s") Iniciar(4);
                    else { Main(); }
                }

                Console.WriteLine("Essas são as tarefas encontradas: \r\n");
                foreach (var item in tarefas)
                {
                    Console.WriteLine($"[{item.Id}] - {item.Descricao} - {(item.Concluida == false ? "Pendente" : "Concluída")}");
                }

                Console.WriteLine("Por favor digite o número da tarefa que quer remover: ");

                int tarefaARemover = Convert.ToInt32(Console.ReadLine());

                var removido = RemoverTarefa(tarefaARemover);

                if (removido == false) Console.WriteLine("Nenhuma tarefa encontrada com o número informado. Gostaria de tentar novamente?");

                resposta = Console.ReadLine();
                if (resposta == "sim" || resposta == "si" || resposta == "s") Iniciar(4);

                else { Main(); }
                break;
            //Finaliza o programa;
            case 5:
                Console.WriteLine("Saindo do sistema...");
                break;
            //Tratativa de valor sem função.
            case > 5:
                Console.WriteLine("Nenhuma funcionalidade encontrada para a opção selecionada. Por favor selecione uma opção válida");
                Main();
                break;
        };
    }

    #region Métodos utilizados para manipulação das tarefas

    private static string AdicionarTarefa(string descricao)
    {
        //Verifica se o arquivo existe e cria caso não exista;
        var path = ArquivoExiste();

        //Realiza a leitura do arquivo;
        var jsonContent = LerArquivo(path);

        //Caso o arquivo esteja vazio, adiciona uma nova lista vazia para manipular posteriormente;
        if (jsonContent == null || string.IsNullOrEmpty(jsonContent))
            jsonContent = JsonConvert.SerializeObject(new List<Tarefa>(), Formatting.Indented);

        //Deserializa o json
        List<Tarefa> tarefas = System.Text.Json.JsonSerializer.Deserialize<List<Tarefa>>(jsonContent)!;

        //Adiciona a nova tarefa e incrementa o ID de acordo com o último ID adicionado;
        tarefas.Add(new Tarefa() { Id = tarefas.Count > 0 ? tarefas.OrderByDescending(c => c.Id).FirstOrDefault().Id + 1 : 0, Descricao = descricao });
        jsonContent = JsonConvert.SerializeObject(tarefas, Formatting.Indented);

        //Realiza a edição do arquivo.
        EditarArquivo(path, jsonContent);

        return "Tarefa adicionada!";
    }

    private static List<Tarefa> ListarTarefas()
    {
        //Verifica se o arquivo existe e cria caso não exista;
        var path = ArquivoExiste();

        var content = LerArquivo(path);

        if (content == null || string.IsNullOrEmpty(content))
            return null;

        List<Tarefa> jsonContent = System.Text.Json.JsonSerializer.Deserialize<List<Tarefa>>(content)!;
        return jsonContent;
    }

    private static bool RemoverTarefa(int id)
    {
        //Verifica se o arquivo existe e cria caso não exista;
        var path = ArquivoExiste();

        var tarefas = ListarTarefas();

        Tarefa tarefaARemover = tarefas.Where(e => e.Id == id).FirstOrDefault();

        if (tarefaARemover == null)
            return false;

        tarefas.Remove(tarefaARemover);

        var jsonContent = JsonConvert.SerializeObject(tarefas, Formatting.Indented);

        EditarArquivo(path, jsonContent);

        return true;
    }

    private static bool ConcluirTarefa(int id)
    {
        //Verifica se o arquivo existe e cria caso não exista;
        var path = ArquivoExiste();

        //Lê e retorna o conteúdo do arquivo
        var content = LerArquivo(path);

        List<Tarefa> tarefas = ListarTarefas();

        Tarefa tarefaConcluida = tarefas.Where(x => x.Id == id).FirstOrDefault();

        if (tarefaConcluida == null) return false;

        //Remove a tarefa "antiga"
        tarefas.Remove(tarefaConcluida);

        tarefaConcluida.Concluida = true;

        //Insere a mesma tarefa mas com o status atualizado
        tarefas.Add(tarefaConcluida);

        var jsonContent = JsonConvert.SerializeObject(tarefas, Formatting.Indented);

        //Atualiza no arquivo
        EditarArquivo(path, jsonContent);

        return true;
    }

    #endregion

    #region Métodos genéricos para manipulação do arquivo

    private static string ArquivoExiste()
    {
        string path = AppDomain.CurrentDomain.BaseDirectory + "data.json";

        bool existe = File.Exists(path);

        //Cria o arquivo caso ele não existe e fecha o processo utilizando o arquivo.
        if (!existe)
            File.Create(path).Close();

        return path;
    }

    private static string LerArquivo(string path)
    {
        if (File.Exists(path))
            return File.ReadAllText(path);

        return null;
    }

    private static string EditarArquivo(string path, string jsonContent)
    {
        if (File.Exists(path))
            File.WriteAllText(path, jsonContent);

        return path;
    }

    #endregion
}