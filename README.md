# IPLChat - Projeto de Tópicos de Segurança

## Autores

- João Afonso Jerónimo de Matos - 2220857
- João Miguel dos Santos Zeferino Fernandes - 2220876
- Rúben Rosa Lisboa - 2220862

## Nota

Devido à informação dada pelo professor relativamente à possibilidade de entregar uma melhoria do projeto até dia 26 de junho, decidimos adiar a implementação de algumas funcionalidades para essa data.

### Estrutura de ficheiros do servidor

```
.
├── Channel.cs
├── Client.cs
├── DB.cs
├── dbschema.sql
├── Logger.cs
├── Program.cs
├── Server.cs
├── Util.cs
```

#### Channel.cs

O ficheiro `Channel.cs` define a representação de um canal de comunicação registado na rede (representa a forma como o servidor interpreta um canal de comunicação). Cada canal de comunicação pode ter mais do que um cliente conectado em simultâneo (semelhante a outros serviços de Chat).

Cada canal é mantido de forma ordenada consoante o seu `relevance score` na rede. Quanto mais relevante for um canal, maior a probabilidade deste ser adicionado ao cache da base de dados. O `relevance score` de um canal é calculado com base nos seguintes parâmetros, cada um com o seu peso:

- Número de utilizadores subscritos ao canal (__50%__)
- Número de vezes que o canal foi requisitado (__30%__)
- Número de requests feitos num intervalo de tempo (__20%__)

Adicionalmente, a relevancia de um canal diminui com o tempo, por um `decay factor` definido (`0.0001` a cada segundo).

```cs
private static float RateChannel(Channel channel)
{
    float decayFactor = (float)Math.Pow(1f - channel._channelRelevanceDecay, TimeSinceLastRequest(channel) / channel._channelRelevanceDecayRate);

    float relevanceScore = (channel._amountOfUsersWeight * channel._clients.Count) +
                           (channel._requestCountWeight * channel._channelRequestCount) +
                           (channel._lastRequestWeight * decayFactor);

    return relevanceScore;
}
```

#### Client.cs

O ficheiro `Client.cs` define a representação de um cliente registado na rede (representa a forma como o servidor interpreta um cliente). Visa facilitar o rastreamento dos comportamentos de cada cliente, bem como as chaves criptográficas associadas a cada um, entre outros dados estatísticos relevantes para o controlo das tentativas de autenticação de modo a evitar ataques de força bruta.

#### DB.cs

O ficheiro `DB.cs` contém a classe `DB` que é responsável por orquestrar o acesso à camada de persistência de dados do servidor.

Oferece otimizações de acesso à base de dados, como por exemplo, o _batching_ de writes para diminuir o número de acessos à base de dados, ou o _caching_ dos reads mais frequentes.

Adicionalmente, esta classe está encarregue de criar a base de dados e gerir os backups da mesma.

As mensagens são agrupadas em _batches_ de um tamanho especificado no ficheiro de configurações do servidor. Quando o número de mensagens num _batch_ atinge o tamanho especificado ou quando o `flush timeout` é atingido, o _batch_ é escrito na base de dados.

A classe `DB` gere também o _caching_ dos canais de comunicação mais relevantes na rede (ver secção "Channel.cs"). Outros tipos de dados críticos como os dados de autenticação dos clientes são acedidos diretamente através da base de dados por motivos de segurança.

#### dbschema.sql

O ficheiro `dbschema.sql` contém o esquema da base de dados do servidor. É importado pelo programa como um _Embedded Resource_ e server para criar a base de dados __SQLite__ e as tabelas necessárias.

#### Logger.cs

O ficheiro `Logger.cs` contém a classe `Logger` que é responsável por gerir os logs do servidor. Os logs são escritos para um ficheiro de texto especificado no ficheiro de configurações do servidor. Adicionalmente, é possível especificar o nível de detalhe dos logs (erros, warnings, informacoes).

É também possível ativar diversos modos de _logging_ com recurso ao comando `log` do servidor. Estes modos de _logging_ são úteis para depurar o servidor e para analisar o seu comportamento (ver secção da Command-Line-Interface do servidor).

#### Program.cs

O ficheiro `Program.cs` contém o método `Main` do servidor. É responsável por inicializar o servidor e por iniciar a sua execução.

#### Server.cs

O ficheiro `Server.cs` contém a implemtação da lógica do servidor. Define a lógica da classe base da biblioteca [ProtoIP](https://joaoajmatos.github.io/ProtoIP/#/Server). Esta classe é responsável por gerir as conexões com os clientes e servir de intermediário para a troca de mensagens.

##### Comandos do servidor

A _Command-Line-Interface_ que acompanha o servidor permite executar comandos para gerir o servidor e para obter informações sobre o seu estado atual.

Os comandos disponíveis são:

- `help` - Mostra a lista de comandos disponíveis
- `stop` - Termina a execução do servidor
- `log` - Ativa ou desativa o _logging_ de erros, warnings e informacoes
- `config` - Mostra a configuração atual do servidor
- `license` - Mostra a licença do servidor
- `snapshot` - Cria um _snapshot_ da base de dados
- `snapshot-list` - Mostra a lista de _snapshots_ da base de dados
- `snapshot-load` - Carrega uma _snapshot_ da base de dados
- `snapshot-revert` - Reverte o estado da base de dados para o estado anterior ao último _snapshot load_
- `clients` - Mostra a lista de clientes conectados ao servidor

> Comandos de Debug

Alguns comandos do servidor só estão disponíveis quando o servidor é executado em modo de debug dentro do ambiente de desenvolvimento do Visual Studio. Estes comandos são:

- `database-wipe` - Apaga a base de dados
- `sql` - Executa uma query SQL na base de dados

A CLI do Servidor oferece ainda uma funcionalidede de _fuzzy search_ para os comandos. Por exemplo, ao executar o comando `helt` o servidor irá informar o utilizador do comando mais próximo que corresponde à pesquisa, neste caso, o comando `help`.

#### Util.cs

O ficheiro `Util.cs` contém uma série de métodos utilitários. Métodos que não se enquadram em nenhuma classe em particular. Como é o caso da implementação do algoritmo de _Levenshtein Distance_ que é usado para implementar a funcionalidade de _fuzzy search_ da CLI do servidor.

### Estrutura de ficheiros do cliente

```
.
├── Client.cs
```

#### Client.cs

O ficheiro `Client.cs` contém a implementação da lógica do cliente. Define a lógica da classe base da biblioteca [ProtoIP](https://joaoajmatos.github.io/ProtoIP/#/Client).