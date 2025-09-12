

# md2visio

>  convert Markdown (`mermaid` / `mmd`) to `Visio` (`vsdx`)

AI-assisted programming simplifies the creation of algorithm flowcharts. Many large language models (LLMs) are capable of generating Markdown (`Mermaid`) flowcharts. However, Markdown editors lack the functionality to edit these flowcharts as intuitively as specialized drawing tools such as `Visio`, which allow for actions like moving nodes or reorganizing layouts.

The tool `md2visio` bridges this gap by converting Markdown (`Mermaid`/`MMD`) into `Visio` (`.vsdx`) format. This enables users to leverage the capabilities of LLMs for initial chart creation while also benefiting from the advanced editing features of `Visio` for further customization and refinement of the flowcharts.

Mermaid example:

```bat
journey
    %% journey test
    title My working day
    section Go to work
      Make tea: 5: Me
      Go upstairs: 3: Me
      Do work: 1: Me, Cat
    section Go home
      Go downstairs: 5: Me
      Sit down: 5: Me
    section Go home
    	Make tea: 3: Me
```

Generated `Visio` graph:

<img src="https://github.com/Megre/md2visio/blob/main/example.png" alt="journey.vssx" style="zoom: 50%;" />



# Usage

## Command Line

```bat
md2visio /I MD_FILE /O OUTPUT_PATH [/V] [/Y]

/I      Specify the path of the input file (.md)
/O      Specify the path/folder of the output file (.vsdx)
/V      Show the Visio application while drawing (optional, default is invisible)
/Y      Quietly overwrite the existing output file (optional, by default requires user confirmation)
/?      Print this help
```

Example:

```bash
md2visio /I D:\figure.md /O D:\
```

It converts all `Mermaid` figures in `figure.md` and outputs `.vssx` files to `D:\`



## Requirements

- `Microsoft Visio 2010` or later. You need install `Visio` since `md2visio `opens `Visio` and automatically draws figures on it.
- [md2visio.vssx](./md2visio/md2visio.vssx). This stencil file is provided by this project ([./md2visio/md2visio.vssx](./md2visio/md2visio.vssx)). It contains basic figure parts used by `md2visio`. You need to put this file to execution file path (or system path). 



# Supported Mermaid Figure

Development plan:

- [x] graph / flowchart
  - [x] themes
- [x] journey
  - [x] themes
- [ ] sequencefigure
- [ ] classfigure
- [ ] statefigure
- [ ] statefigure-v2
- [ ] erfigure
- [ ] gantt
- [x] pie
  - [x] themes
- [ ] quadrantChart
- [ ] requirementfigure
- [ ] gitGraph
- [ ] C4Context
- [ ] mindmap
- [ ] timeline
- [ ] zenuml
- [ ] sankey-beta
- [ ] xychart-beta
- [ ] block-beta
- [x] packet-beta
  - [x] themes
- [ ] kanban
- [ ] architecture-beta
- [x] Configuration
  - [x] frontmatter
  - [x] directive



# Contributing

## Introduction

The implementation of the project consists of several parts:

**Syntax Parsing.** When parsing Mermaid syntax, the project treats the Mermaid text as a state machine composed of different fields. By continuously looking ahead and backtracking, it identifies the components that constitute different types of figures. The initial state for every figure is always `SttMermaidStart` (three or more **Backtick**), and the final state is always `SttMermaidClose` (three or more **Backtick**).

**Building the data structure for figures.** The parsed components are structured into a well-defined format, preparing the data for rendering.

**Rendering in Visio.** To enable rendering, an `md2visio.vssx` file was created. It contains reusable graphical elements which the rendering program repeatedly uses to construct the figures.



## Build

The project is built with Visual Studio 2022. All dependency solution files are uploaded.



## Depending Packages

- WonderCircuits.Microsoft.Office.Interop.Visio
- WonderCircuits.Microsoft.Office.Core
- WonderCircuits.Microsoft.Vbe.Interop
- YamlDotNet
