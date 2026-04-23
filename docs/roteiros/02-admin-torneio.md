# Roteiro Completo do Administrador do Torneio

## Objetivo do vídeo

Demonstrar a operação completa do Administrador do Torneio na retaguarda web, incluindo cadastros, sorteio, operação, financeiro, relatórios e controles emergenciais.

## Cenário recomendado

- Usar `Torneio BTS Sport Fishing 2026` para regras sem sorteio.
- Usar `Amigos da Pesca 2026` e `Rei dos Mares 2026` para fluxos com sorteio.
- Manter um torneio em `GrupoEquipe` para demonstrar `Grupos`.
- Preparar dados de exemplo para embarcações, membros, itens, fiscais, patrocinadores, capturas, prêmios e financeiro.

## Sequência sugerida de gravação

1. Acessar a URL do torneio e fazer login como Admin do Torneio.
2. Mostrar o painel inicial, status do torneio e atalhos principais.
3. Destacar a ordem dos cadastros:
   - Embarcações
   - Fiscais
   - Membros
   - Grupos, somente em `GrupoEquipe`
   - Itens
   - Patrocinadores
4. Demonstrar as transições de status entre `Aberto`, `Liberado` e `Finalizado`, inclusive retorno para `Aberto`.
5. Abrir `Clonar torneio` e explicar o reaproveitamento de configuração.
6. Em `Embarcações`, listar, criar, editar e remover, mostrando:
   - foto da embarcação
   - foto do capitão
   - capitão
   - vagas
   - custo e status financeiro, quando o módulo financeiro estiver habilitado
7. Em `Fiscais`, listar, criar, editar e remover, destacando:
   - foto
   - usuário e senha
   - vínculo com múltiplas embarcações via checklist de embarcações
8. Em `Membros`, listar, criar, editar e remover, mostrando:
   - foto
   - celular formatado
   - tamanho da camisa, somente quando o módulo financeiro estiver habilitado
   - usuário e senha opcionais, somente quando o acesso público de membro estiver habilitado
9. Em `GrupoEquipe`, abrir `Grupos` no menu de cadastros, criar grupos, adicionar membros e explicar que cada grupo participa do sorteio de embarcações.
10. Em `Itens`, listar, criar, editar e remover, destacando imagem, fator multiplicador e comprimento mínimo opcional.
11. Em `Patrocinadores`, cadastrar com imagem e ao menos um destino opcional: Instagram, Facebook, site ou Zap.
12. Abrir a tela pública do torneio e mostrar:
   - patrocinadores com imagem maior
   - acesso do admin
   - acesso do membro apenas quando habilitado
   - registro público e recuperação de senha do membro apenas quando habilitados
13. Em `Capturas`, mostrar:
   - visualização das capturas
   - foto ampliável
   - alteração do tamanho com log
   - invalidação, revalidação e remoção
14. Em `Sorteio`, usar um torneio com sorteio habilitado para:
   - mostrar pré-condições
   - executar a animação
   - confirmar o resultado
   - limpar o sorteio, se necessário
15. Em `GrupoEquipe`, reforçar que o cadastro de grupos vem antes da execução do sorteio.
16. Em um torneio com `Modo de Sorteio = Nenhum`, mostrar que o menu `Sorteio` não existe.
17. Em `Prêmios`, cadastrar premiação por embarcação e/ou membro conforme a configuração do torneio.
18. Em `Relatórios`, emitir:
   - por embarcação
   - por membro
   - ganhadores
   - maiores capturas, somente para torneio do tipo pesca
19. Em `Reorganização Emergencial`, explicar que o menu aparece após `Relatórios`, é uso excepcional e exige motivo, confirmação e log.
20. Em `Financeiro`, mostrar a ordem do módulo:
   - Visão geral
   - Configuração
   - Cobranças
   - Custos
   - Produtos extras
   - Doações
   - Checklist
   - Relatórios
21. Em `Financeiro > Visão geral`, apresentar os indicadores calculados no backend.
22. Em `Financeiro > Configuração`, definir valor por membro, parcelas, vencimento e taxa de inscrição opcional, explicando a confirmação quando já existe configuração anterior.
23. Em `Financeiro > Cobranças`, mostrar filtros por membro, tipo, não pagas e inadimplentes, além de edição, pagamento e comprovante.
24. Em `Financeiro > Custos`, cadastrar custos com categoria em português, quantidade, valor, vencimento e composição do custo total.
25. Em `Financeiro > Produtos extras`, cadastrar produtos e registrar venda para membro.
26. Em `Financeiro > Doações`, registrar doações com patrocinador opcional, em dinheiro ou produto.
27. Em `Financeiro > Checklist`, cadastrar itens e escolher o responsável entre os admins do torneio.
28. Em `Financeiro > Relatórios`, mostrar os relatórios financeiros e o gráfico de linha.
29. Finalizar reforçando que a operação completa ocorre dentro da plataforma, sem planilhas externas.

## Pontos de narração importantes

- O menu `Grupos` só existe quando o modo de sorteio é `GrupoEquipe`.
- O menu `Sorteio` desaparece quando o modo de sorteio é `Nenhum`.
- Patrocinadores são exibidos na tela pública do torneio, não na home interna do admin.
- O módulo financeiro inteiro pode ser ocultado por configuração do torneio.
- Reorganização Emergencial é uma exceção operacional e sempre deve ser registrada.

## Resultado esperado do vídeo

Ao final, o espectador deve entender como o Admin do Torneio conduz o evento completo na retaguarda, desde a configuração até a operação, o financeiro e os relatórios.
