# ZeroToHeroUnheard

Um mod para SPT 4.0 que adiciona um novo perfil de início do zero, mantendo os benefícios da edição Unheard.

## Descrição

Este mod cria um novo perfil chamado **"Zero To Hero Unheard"** que permite começar do zero absoluto, mas mantendo os benefícios da edição Unheard (Gamma case e tamanho de stash).

## Funcionalidades

- ✅ **Perfil baseado no Unheard**: Mantém o status da edição Unheard (Gamma case e stash size)
- ✅ **Inventário inicial básico**: Começa apenas com uma faca e equipamentos essenciais
- ✅ **Traders desbloqueados**: Todos os traders começam com nível 1 de lealdade
- ✅ **Skills zeradas**: Todas as habilidades começam do zero, sem vantagens iniciais
- ✅ **Sem dinheiro inicial**: Começa sem rublos, começando do zero financeiro

## Requisitos

- SPT versão ~4.0.0
- Perfil "Unheard" deve existir no banco de dados do SPT

## Instalação

1. Copie a pasta do mod para o diretório `user/mods/` do seu SPT
2. Compile o projeto ou use o DLL já compilado
3. Inicie o servidor SPT
4. O novo perfil "Zero To Hero Unheard" estará disponível na seleção de perfis

## Configuração

Você pode personalizar o mod editando os arquivos JSON na pasta `config/`:

- `bear_inventory.json` - Inventário inicial para facção BEAR
- `usec_inventory.json` - Inventário inicial para facção USEC
- `traders.json` - Configuração de standing inicial dos traders
- `skill_issue.json` - Configuração de skills iniciais
- `descLocale.json` - Descrição do perfil

## Autor

**jero**

## Licença

MIT

