# Once Upon A Bite — C# Windows Forms + MySQL

## Requisitos
- Windows
- Visual Studio 2022
- .NET 8 SDK
- MySQL Server e MySQL Workbench

## 1. Banco de dados
Execute primeiro no MySQL Workbench o script SQL de criação do banco `once_upon_a_bite` enviado na conversa.

O projeto espera as tabelas:
- clientes
- entregadores
- produtos
- pedidos
- pedido_itens

## 2. Configurar conexão
Abra `Data/Db.cs` e altere:

`Password=SUA_SENHA;`

para a senha do usuário MySQL. Se seu usuário não for `root`, altere também `User ID`.

Exemplo:
`Server=localhost;Port=3306;Database=once_upon_a_bite;User ID=root;Password=123456;`

## 3. Abrir o projeto
1. Extraia o ZIP.
2. Abra `OnceUponABite.csproj` no Visual Studio.
3. Aguarde a restauração do pacote NuGet `MySqlConnector`.
4. Confirme que o banco MySQL está iniciado.
5. Pressione F5.

## Funcionalidades
- FrmClientes: inserir, consultar, alterar e excluir.
- FrmEntregadores: inserir, consultar, alterar e excluir.
- FrmProdutos: consultar, alterar preço/estoque e excluir.
- FrmPedidos: consultar pedidos, visualizar itens, alterar status e excluir.

## Observações
- Os formulários são montados por código; não precisam de arquivos Designer.
- As consultas usam parâmetros.
- A exclusão de um pedido apaga seus itens por causa do `ON DELETE CASCADE`.
- A exclusão de cliente ou entregador mantém os pedidos, removendo apenas o vínculo (ON DELETE SET NULL).
- A exclusão de produto mantém o nome e o preço histórico no item do pedido.
- O formulário de pedidos respeita as permissões solicitadas: não cria pedidos nem altera itens.
- A conexão deve ser ajustada para a senha real do seu MySQL.
