// ==============================================================
// FUNÇÃO DE PESQUISA INTELIGENTE (UNIVERSAL)
// ==============================================================
// O que ela faz:
// 1. Ignora acentos (João = Joao)
// 2. Ignora maiúsculas (Admin = admin)
// 3. Pesquisa em todas as colunas da tabela
// ==============================================================

function ativarPesquisa(idDoCampoDeTexto, idDaTabela) {
    // Tenta encontrar os elementos na tela
    var input = document.getElementById(idDoCampoDeTexto);
    var tabela = document.getElementById(idDaTabela);

    // Se não achar (ex: mudou de página), para o código para não dar erro
    if (!input || !tabela) return;

    // Fica vigiando quando você digita algo
    input.addEventListener('keyup', function () {
        // Pega o texto, tira acentos e deixa minúsculo
        var termo = this.value.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();

        // Pega as linhas da tabela (tbody tr)
        var linhas = tabela.querySelectorAll('tbody tr');

        linhas.forEach(function (linha) {
            // Pega o texto da linha inteira, tira acentos e deixa minúsculo
            var textoDaLinha = linha.textContent.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();

            // Verifica se o que você digitou existe na linha
            if (textoDaLinha.indexOf(termo) > -1) {
                linha.style.display = ""; // Mostra a linha
            } else {
                linha.style.display = "none"; // Esconde a linha
            }
        });
    });
}