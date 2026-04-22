# Roteiro Completo do Administrador do Torneio

## Objetivo do video

Demonstrar todas as funcionalidades do Administrador do Torneio na retaguarda web, da configuracao inicial ate o encerramento operacional do evento.

## Cenario recomendado

- Usar `Torneio BTS Sport Fishing 2026` para regras sem sorteio.
- Usar `Amigos da Pesca 2026` e `Rei dos Mares 2026` para fluxos com sorteio.
- Manter dados de exemplo para embarcacoes, pescadores, peixes, fiscais, patrocinadores, capturas, premios e financeiro.

## Sequencia sugerida de gravacao

1. Acessar a URL do torneio e fazer login como Admin do Torneio.
2. Mostrar painel, status atual, patrocinadores exibidos e atalhos principais.
3. Demonstrar transicoes entre `Aberto`, `Liberado` e `Finalizado`, inclusive retorno para aberto.
4. Abrir Clonagem de torneio e explicar o reaproveitamento de configuracoes.
5. Entrar na configuracao do torneio e destacar modulo financeiro, premiacao por embarcacao ou pescador e registro publico de pescador.
6. Listar, criar, editar e remover fiscais, incluindo foto, usuario, senha e vinculacao a multiplas embarcacoes.
7. Listar, criar, editar e remover embarcacoes, mostrando foto da embarcacao, foto do capitao, capitao, vagas e, quando aplicavel, custo e status financeiro.
8. Quando houver distribuicao por sorteio, entrar na tela interna da embarcacao para vincular e remover pescadores.
9. No torneio com modo `Nenhum`, mostrar ausencia do menu Sorteio e da lista/quantidade de pescadores por embarcacao.
10. Listar, criar, editar e remover pescadores, destacando foto, celular formatado, tamanho da camisa quando o financeiro estiver ativo e usuario/senha opcionais.
11. Listar, criar, editar e remover peixes, destacando imagem, fator multiplicador e comprimento minimo opcional.
12. Cadastrar patrocinadores com imagem e pelo menos um destino, mostrando as flags de exibicao na tela inicial e nos relatorios.
13. Abrir a tela publica do torneio e mostrar patrocinadores, login do admin, login do pescador, registro publico e recuperacao de senha do pescador quando habilitados.
14. Abrir Capturas e mostrar fotos, alteracao de tamanho com log, invalidacao, revalidacao e remocao.
15. Abrir Reorganizacao emergencial, informar motivo, digitar a confirmacao e concluir a troca.
16. Nos torneios com sorteio, abrir o modulo, mostrar pre-condicoes, executar a animacao, confirmar resultado, ajustar se necessario e limpar o resultado.
17. Cadastrar premios por posicao e explicar premiacao por embarcacao e/ou pescador.
18. Abrir Financeiro - Visao geral e apresentar os indicadores calculados no backend.
19. Em Financeiro - Configuracao, definir valor por pescador, quantidade de parcelas, vencimento e taxa de inscricao opcional.
20. Em Financeiro - Gerenciamento de parcelas, gerar para novos pescadores, regerar para selecionados e regerar geral com cautela.
21. Em Financeiro - Cobrancas, filtrar por pescador, tipo, nao pagas e inadimplentes; editar cobranca, vencimento, observacao, pagamento e comprovante.
22. Em Financeiro - Custos, cadastrar custos com categoria em portugues, quantidade, valor, vencimento e composicao do custo total.
23. Em Financeiro - Produtos extras, cadastrar extras e registrar vendas por pescador.
24. Em Financeiro - Doacoes, registrar doacoes de patrocinadores em dinheiro ou produto.
25. Em Financeiro - Checklist, cadastrar itens e escolher o responsavel entre os admins do torneio.
26. Em Financeiro - Relatorios, mostrar relatorios e grafico de linha de recebimentos e pagamentos previstos.
27. Em Relatorios gerais, emitir relatorios por embarcacao, por pescador, ganhadores e maiores capturas, com patrocinadores quando habilitados.
28. Finalizar reforcando que toda a operacao ocorre sem planilhas externas.

## Pontos de narracao importantes

- O menu Sorteio desaparece quando o modo de sorteio e `Nenhum`.
- Reorganizacao emergencial exige motivo, confirmacao explicita e log.
- O modulo financeiro pode ser ocultado por configuracao do torneio.
- As imagens ajudam na conferencia operacional.
- O acesso do pescador depende de credenciais opcionais e pode ter recuperacao por SMS quando habilitado.

## Resultado esperado do video

Ao final, o espectador deve entender como o Admin do Torneio conduz o evento completo na retaguarda, incluindo cadastros, operacao, financeiro, sorteio, premiacao e relatorios.
