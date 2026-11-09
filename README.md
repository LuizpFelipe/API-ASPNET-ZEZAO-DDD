# Zezão Futsal — API (Back-end)

API REST do sistema de gestão da escolinha de futsal **Zezão Futsal**: cadastro de alunos, turmas e categorias, contratos com geração de PDF, controle financeiro (parcelas e inadimplência), chamada/presença, reposições e campeonatos internos com times, jogos e ranking.

Front-end (React): <https://github.com/LuizpFelipe/REACT-ZEZAO>

## Objetivo

Substituir o controle manual (planilhas e papel) da escolinha por um sistema único, onde a **Coordenação** administra alunos, contratos e cobranças e o **Professor** faz a chamada das suas turmas e conduz os campeonatos.

## Tecnologias

- **.NET 10** / ASP.NET Core Web API
- **Entity Framework Core 10** + **PostgreSQL** (Npgsql) com Migrations
- **Autenticação JWT** (Bearer) com perfis `Coordenador` e `Professor`
- **Swagger / OpenAPI** para documentação
- **xUnit** para testes automatizados
- Deploy no **Railway**

## Estrutura (DDD em camadas)

```
├── Api                 # Controllers, Program.cs (DI, JWT, CORS, Swagger), serviços de apoio
├── Application         # Services (casos de uso), DTOs (Request/Response), validações, regras de campeonato
├── Domain              # Entidades e interfaces (repositórios e serviços de domínio) — sem dependências externas
├── Infrastructure      # DbContext, mapeamentos EF, Migrations, Repositories, segurança (hash), e-mail, PDF, storage
├── Tests               # Testes de serviços, entidades, validações, campeonato e utilitários
└── API-ASPNET-ZEZAO.slnx
```

Dependências: `Api → Application → Domain` e `Infrastructure → Domain`. O `Domain` não depende de nenhuma outra camada; as implementações concretas são ligadas por injeção de dependência em `Api/Program.cs`.

Fluxo de uma requisição: **Front → Controller → Service (regras de negócio) → Repository → PostgreSQL**.

## ▶ Como executar a API

### Pré-requisitos
- [.NET SDK 10](https://dotnet.microsoft.com/download)
- PostgreSQL em execução

### 1. Configuração (segredos fora do Git)

O `appsettings.json` guarda apenas valores padrão. Os segredos vão em **User Secrets** (recomendado) ou em `Api/appsettings.Development.json` (ignorado pelo Git; use o `.example` como modelo).

```bash
cd Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=ZezaoFutsalDB;Username=postgres;Password=SUA_SENHA"
dotnet user-secrets set "Jwt:Key" "UMA_CHAVE_SECRETA_COM_PELO_MENOS_32_CARACTERES"
dotnet user-secrets set "UsuariosIniciais:SenhaInicial" "SENHA_INICIAL_DO_COORDENADOR"
```

| Chave | Para que serve |
|---|---|
| `ConnectionStrings:DefaultConnection` | Conexão com o PostgreSQL |
| `Jwt:Key` / `Jwt:Issuer` / `Jwt:Audience` | Assinatura e validação do token JWT |
| `UsuariosIniciais:EmailDoCoordenador` / `EmailDoProfessor` / `SenhaInicial` | Usuários criados na primeira execução |
| `Cors:Origens` | URLs do front permitidas (ex.: `http://localhost:5173`) |
| `Banco:MigrarAoIniciar` | `true` aplica as migrations ao iniciar |
| `Seed:Habilitado` | `true` cria os usuários iniciais (em Development roda sempre) |
| `Seed:DadosDeDemonstracao` | `true` popula alunos, turmas, contratos, parcelas e campeonatos de exemplo (aditivo) |
| `Seed:DataDaApresentacao` | Data de referência dos dados de demonstração |
| `Swagger:Habilitado` | `true` expõe o Swagger fora de Development |
| `Smtp:*` | Envio de e-mail (recuperação de senha e cobrança) |
| `Contrato:*` | Valor e quantidade de parcelas, dia de vencimento e dados da escola no contrato |
| `CobrancaAutomatica:*` | Rotina automática de avisos de cobrança |
| `Presenca:DiasLimiteEdicaoProfessor` | Prazo (dias) para o professor editar uma chamada |

### 2. Banco de dados

```bash
dotnet ef database update --project Infrastructure --startup-project Api
```
(ou use `Banco:MigrarAoIniciar=true`).

### 3. Rodar

```bash
dotnet run --project Api
```
A API sobe em `http://localhost:5260`.

### Produção (Railway)

Em produção as chaves são passadas como variáveis de ambiente (`Jwt__Key`, `ConnectionStrings__DefaultConnection`, `Seed__Habilitado`, `Swagger__Habilitado`, ...).

## Swagger

Com `Swagger:Habilitado=true` (ou em Development): `http://localhost:5260/swagger`.

## Autenticação

1. `POST /api/Auth/login` com e-mail e senha devolve um **token JWT**.
2. Enviar nas demais chamadas o cabeçalho `Authorization: Bearer <token>`.
3. Perfis:
   - **Coordenador**: acesso total (alunos, turmas, categorias, professores, contratos, financeiro, usuários).
   - **Professor**: vê suas turmas, faz a chamada e gerencia campeonatos.
4. Recuperação de senha por e-mail: `recuperar-senha` → `validar-codigo` → `redefinir-senha`.
5. Sem token → `401`; perfil sem permissão → `403`.

As respostas seguem o envelope `{ "message": "...", "status": 200, "data": ... }`.

## Principais endpoints

| Área | Rotas | Perfil |
|---|---|---|
| Autenticação | `POST /api/Auth/login`, `recuperar-senha`, `validar-codigo`, `redefinir-senha` | público |
| Alunos | `GET/POST/PUT/DELETE /api/Aluno`, `POST /api/Aluno/promover-categorias` | leitura: logado; escrita: Coordenador |
| Categorias | `GET/POST/PUT/DELETE /api/Categoria` | escrita: Coordenador |
| Turmas | `GET/POST/PUT/DELETE /api/Turma`, `GET /api/Turma/categoria/{id}`, `PATCH /api/Turma/{id}/inativar`, `PATCH /api/Turma/{id}/reativar` | escrita: Coordenador |
| Professores | `GET/POST/PUT/DELETE /api/Professor` | leitura: logado; escrita: Coordenador |
| Coordenadores | `/api/coordenadores` (CRUD) | Coordenador |
| Usuários | `PUT /api/Usuarios/{id}/resetar-senha` | Coordenador |
| Contratos | `GET /api/Contrato`, `GET /api/Contrato/{id}/pdf`, `PUT /api/Contrato/{id}` | Coordenador |
| Financeiro | `GET /api/Financeiro/resumo`, `GET /api/Parcelas`, `PUT /api/Parcelas/{id}/pagamento`, `POST /api/Parcelas/{id}/cobranca`, `POST /api/Financeiro/rotina` | Coordenador |
| Presença | `GET /api/Professores/minhas-turmas`, `GET/POST /api/Turmas/{id}/chamada`, `GET /api/Turmas/{id}/chamada-mensal` | Professor e Coordenador |
| Reposições | `GET/POST /api/Reposicoes` | Coordenador |
| Campeonatos | `GET/POST/PUT/DELETE /api/Campeonatos`, inscrições em `/api/Campeonatos/{id}/participacoes`, `PUT /api/Participacoes/{id}` | escrita: Coordenador e Professor |
| Competição | `/api/Campeonatos/{id}/elegiveis`, `equipes`, `equipes/aleatorias`, `jogos/gerar`, `jogos`, `ranking`; `/api/Jogos/{id}/estatisticas`, `/api/Jogos/{id}/finalizar` | consulta: logado; escrita: Coordenador e Professor |

A lista completa e os modelos de entrada/saída estão no Swagger.

## Regras de negócio em destaque

- Aluno duplicado (mesmo nome e data de nascimento) é recusado; reinscrever um aluno inativo o reativa.
- Inativar um aluno cancela seus contratos ativos.
- Inativar uma turma exige que não haja alunos, ou que eles sejam transferidos para outra turma da mesma categoria.
- Turma não pode ter conflito de nome na categoria nem de dia/horário para o mesmo professor.
- Contrato gera as parcelas conforme `Contrato:*`; inadimplência é calculada pelas parcelas vencidas.
- Campeonato: time com 5 jogadores, um por posição (Goleiro, Fixo, Ala direito, Ala esquerdo, Pivô); empate exige gol de ouro; chaves com "bye" quando o número de times é ímpar; ranking por pontuação de gols, assistências, passes, defesas e disciplina.

## Testes

```bash
dotnet test
```
Cobrem serviços da camada Application, entidades, validações, regras de campeonato e utilitários (170+ casos).

## Instruções de uso do sistema

1. Entrar como **Coordenador** → cadastrar categorias, professores e turmas.
2. Cadastrar o aluno (**Novo Aluno**) → o contrato e as parcelas são gerados.
3. Acompanhar **Financeiro** e **Inadimplentes**; registrar pagamentos e disparar cobranças.
4. Entrar como **Professor** → abrir a turma, fazer a **chamada** (diária ou mensal).
5. **Campeonato**: criar, inscrever alunos, montar os times (manual ou aleatório), gerar os jogos, lançar as estatísticas, finalizar e consultar o ranking.
