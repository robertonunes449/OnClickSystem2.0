# OnClickSystem 2.0 - (Premium)

<div align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8"/>
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" alt="C#"/>
  <img src="https://img.shields.io/badge/ASP.NET_MVC-5C2D91?style=for-the-badge&logo=.net&logoColor=white" alt="ASP.NET MVC"/>
  <img src="https://img.shields.io/badge/Windows_Forms-0078D6?style=for-the-badge&logo=windows&logoColor=white" alt="Windows Forms"/>
  <img src="https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white" alt="SQL Server"/>
</div>

<br>

Bem-vindo ao repositório oficial do **OnClickSystem 2.0**[cite: 4]. Uma plataforma premium, robusta e escalável desenvolvida para o gerenciamento completo de operações comerciais, e-commerce e redes de afiliados (Marketing Multinível - MMN)[cite: 4]. 

Inspirado na inteligência, adaptabilidade e agilidade da **Kitsune** (a mítica raposa de nove caudas da mitologia japonesa), o sistema ostenta uma identidade visual premium baseada em Preto, Branco e Dourado[cite: 4]. Ele foi projetado do zero para suportar alto volume de transações, mantendo uma base de código limpa e de fácil manutenção[cite: 4].

---

## Arquitetura e Filosofia de Engenharia

O OnClickSystem 2.0 não é apenas um software de vendas; é um ecossistema corporativo desenhado sob princípios rigorosos de engenharia de software[cite: 4]:

*   **Abordagem Pragmatista (Zero DTOs):** Em uma decisão arquitetural focada em velocidade de entrega e redução de *boilerplate*, a camada de *Data Transfer Objects* (DTOs) foi completamente eliminada[cite: 4]. O sistema trafega as entidades de domínio diretamente entre o banco de dados e as interfaces, simplificando o fluxo de dados e acelerando a implementação de novas *features*[cite: 4].
*   **Integridade Inegociável:** A base da rede multinível é a veracidade dos seus usuários. O sistema implementa validações rigorosas direto no banco de dados, tornando o **CPF e o Telefone estritamente obrigatórios** no momento do cadastro[cite: 4]. Não existem "usuários fantasmas"[cite: 4].
*   **Design Híbrido (Web, API e Desktop):**
    *   **`OnClickSystem.Web` (MVC):** Interface voltada para a experiência do cliente final, com navegação responsiva para a loja e visualização da rede de afiliados[cite: 4].
    *   **`OnClickSystem.API`:** Camada de serviços RESTful preparada para futuras integrações com aplicativos mobile e plataformas de terceiros[cite: 4].
    *   **`OnClickSystem.Desktop` (WinForms):** O painel de comando do administrador[cite: 4]. Uma aplicação robusta para *back-office* que garante estabilidade de execução via `System.Windows.Forms.Application.Run` e gerenciamento de estado isolado[cite: 4].

---

## ⚙️ Módulos Principais e Funcionalidades

### 👥 1. Motor de Rede e Afiliados (MMN)
O núcleo lógico mantido pelo `RedeService` e `UsuarioService` gerencia a complexidade da árvore de usuários[cite: 4].
*   **Árvore Autorreferenciada:** Os usuários são vinculados através das chaves de `Patrocinador` e `Indicados`, permitindo o crescimento orgânico e infinito da rede[cite: 4].
*   **Nivelamento Dinâmico em Tempo Real:** O sistema calcula a profundidade de cada afiliado (`NivelNaRede`) dinamicamente, otimizando o mapeamento sem a necessidade de tabelas intermediárias complexas[cite: 4].
*   **Autenticação e Sessão:** Gerenciada pelo `AuthService`, provendo segurança robusta via Cookies criptografados[cite: 4].

### 🛍️ 2. Loja Virtual e E-commerce
*   **Gestão de Kits:** Catálogo completo onde o administrador pode cadastrar pacotes de produtos (`Kits`), configurando URLs de imagens para vitrines atrativas[cite: 4].
*   **Carrinho de Alta Performance:** O carrinho de compras utiliza o armazenamento em sessão temporária (`AddSession`), com limite de inatividade de 30 minutos, evitando reserva indevida de estoque[cite: 4].
*   **Checkout Fluido:** Geração de `Pedidos` atrelados diretamente ao histórico do cliente após a finalização do carrinho[cite: 4].

### 💰 3. Ecossistema Financeiro e PIX
O `FinanceiroService` e o `ComissaoService` formam o coração econômico do projeto, focado na liberdade do afiliado[cite: 4].
*   **Sem Limites, Sem Taxas:** Configuração agressiva de mercado onde **não há limites de saques diários e as taxas de solicitação foram zeradas**[cite: 4]. O dinheiro do afiliado é totalmente livre[cite: 4].
*   **Integração PIX Nativa:** Os usuários podem solicitar resgates (`SolicitacaoSaqueDTO`) utilizando suas chaves PIX cadastradas no perfil[cite: 4].
*   **Motor de Comissionamento:** Rateio automático de bônus na rede ascendente após cada venda de Kit, obedecendo regras estritas de níveis[cite: 4].
*   **Ledger Imutável:** Todas as operações geram registros na tabela de `Transacoes`, funcionando como um extrato bancário à prova de fraudes[cite: 4].

### 🛡️ 4. Painel de Controle Desktop (Admin)
Desenvolvido em Windows Forms para máxima segurança e controle gerencial[cite: 4]:
*   **Dashboards Executivos:** Gráficos e indicadores de faturamento e crescimento gerados no `FormDashboard`[cite: 4].
*   **Gestão Centralizada:** Telas modulares para controle de finanças (`FormAdminFinanceiro`), usuários (`FormAdminUsuarios`) e auditoria de logs (`FormAdminLogs`)[cite: 4].
*   **Auditoria Contínua:** Todas as ações críticas (ex: aprovação de saques) são rastreadas internamente[cite: 4].

---

## 🛠️ Stack Tecnológica
*   **Back-end:** C#, .NET 8, ASP.NET Core MVC, Web API[cite: 4].
*   **Front-end (Web):** HTML5, CSS3, JavaScript, Bootstrap[cite: 4].
*   **Front-end (Admin):** Windows Forms (WinForms) com controles customizados[cite: 4].
*   **Banco de Dados:** Microsoft SQL Server com Entity Framework Core (Code-First)[cite: 4].
*   **Design/UI:** Paleta *Premium* (Preto, Branco, Dourado) e iconografia vetorial[cite: 4].

---

## 🚀 Guia de Instalação e Execução

### Pré-requisitos
*   [.NET 8 SDK](https://dotnet.microsoft.com/download) instalado.
*   [SQL Server](https://www.microsoft.com/sql-server) (LocalDB ou instância dedicada).

### Passo a Passo

1.  **Clone o repositório:**
    ```bash
    git clone [https://github.com/robertonunes449/onclicksystem2.0.git](https://github.com/robertonunes449/onclicksystem2.0.git)
    cd onclicksystem2.0
    ```

2.  **Configuração da String de Conexão:**
    *   No projeto Web/API, abra o `appsettings.json` (ou `appsettings.Development.json`) e atualize a propriedade `DefaultConnection` com as credenciais do seu servidor SQL[cite: 4].
    *   No projeto Desktop (`OnClickSystem.Desktop`), verifique a classe `ConfiguracaoBanco.cs` para garantir que o *app* de retaguarda aponte para a mesma base de dados[cite: 4].

3.  **Aplicar Migrações (Criar o Banco de Dados):**
    Abra o terminal na raiz da solução e execute:
    ```bash
    dotnet ef database update --project OnClickSystem.Application --startup-project OnClickSystem.API
    ```

4.  **Inicialização:**
    *   **Para a Loja/Web:** Execute o projeto Web/API:
        ```bash
        dotnet run --project OnClickSystem.API
        ```
    *   **Para o Painel Admin:** Compile e inicie o projeto `OnClickSystem.Desktop` via Visual Studio ou terminal[cite: 4].

---

## 🗺️ Roadmap de Desenvolvimento

O projeto segue um ciclo de vida ágil. Abaixo, o status atual do que foi entregue e a visão de futuro:

- [x] **Fase 1: Fundações, Arquitetura e Segurança**
  - [x] Modelagem Code-First avançada com Entity Framework Core[cite: 4].
  - [x] Limpeza estrutural: Remoção de DTOs para acesso direto a dados (Domain-Driven pragmático)[cite: 4].
  - [x] Travas de integridade: CPF e Telefone tornados campos estritamente obrigatórios no banco[cite: 4].
  - [x] Autenticação e gestão de sessão configurados de ponta a ponta[cite: 4].

- [x] **Fase 2: Motor de Vendas e Hierarquia (MMN)**
  - [x] Carrinho de compras com temporizador de sessão (30 min) e cache[cite: 4].
  - [x] Módulo de catálogos e Kits atrelados a URLs de imagens[cite: 4].
  - [x] Lógica de construção da árvore (Patrocinador -> Indicado) e cálculo dinâmico de níveis[cite: 4].

- [x] **Fase 3: Fluxo Financeiro Descentralizado**
  - [x] Políticas de saque pró-usuário: **Zero limites diários, zero taxas de resgate**[cite: 4].
  - [x] Módulo completo de requisição de saques via PIX[cite: 4].
  - [x] Motor de Comissionamento automático (distribuição de bônus por níveis)[cite: 4].
  - [x] Histórico/Ledger imutável de transações financeiras[cite: 4].

- [ ] **Fase 4: Expansão, Automação e UX (Próximos Passos)**
   - [ ] **Gateways de Pagamento:** Integração com APIs externas (Mercado Pago, Stripe) para baixa automática de faturas e processamento automatizado das filas de PIX.
  - [ ] **Evolução do Dashboard Desktop:** Implementação de novos relatórios dinâmicos de mapa de calor da rede de afiliados.

---
*Desenvolvido com ☕, código limpo e a astúcia de uma Kitsune.*
