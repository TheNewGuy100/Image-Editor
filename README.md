![Editor de imagem: ferramentas à esquerda, imagem em preto e branco no centro e o console de ações aberto na base](docs/preview.png)

# Editor de imagem

Editor de imagens desktop feito em C# com Windows Forms, para estudar processamento digital de imagens. Cada filtro é implementado à mão, pixel a pixel, sem bibliotecas de imagem.

## O que tem

| Ferramenta | O que faz |
| --- | --- |
| **Girar 90° / Espelhar ↔ ↕** | Transformações geométricas |
| **Conversão de Preto/Branco** | Mistura a imagem original com a versão em tons de cinza, controlada por um slider |
| **Reduzir ruído** | Filtro de mediana por canal, com janelas de 3x3, 5x5, 7x7 e 15x15 |
| **Equalização** | Histograma em 16 níveis com porcentagens, acumulado e novos níveis, mostrado no console |
| **Bordas (Roberts)** | Detecção de bordas pelo gradiente cruzado de Roberts |
| **Restaurar original** | Volta para a imagem importada |

Importar e exportar aceitam BMP, JPG e PNG (menu **Arquivo**).

## Interface

- **Console de Ações** recolhível na base da janela: **Ctrl+J** abre e fecha. Mostra mensagens com horário e cor (informação, sucesso, aviso e erro) e avisa quando chega algo novo com o painel fechado.
- Janela redimensionável, com tema escuro.
- Botões sem imagem carregada avisam no console em vez de falhar.

## Como rodar

Requer Windows e o .NET Framework 4.7.2. Abra `TheEditor.sln` no Visual Studio e execute (F5), ou compile pela linha de comando:

```
msbuild TheEditor.sln /p:Configuration=Release
```

A imagem `Lawrence Roberts.png` na raiz serve para testar os filtros.

## Estrutura

| Arquivo | Função |
| --- | --- |
| `TheEditor/ImageHandler.cs` | Filtros e transformações sobre a imagem |
| `TheEditor/Histogram.cs` | Cálculo do histograma e da equalização |
| `TheEditor/MainWindow.cs` | Eventos dos botões e menus |
| `TheEditor/MainWindow.Layout.cs` | Layout da janela e criação do console |
| `TheEditor/MainWindow.Theme.cs` | Tema escuro, textos e dicas |
| `TheEditor/ConsoleDrawer.cs` | Painel recolhível de mensagens |
