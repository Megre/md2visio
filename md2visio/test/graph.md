## Comment

`````mermaid
  %%  {init: {"flowchart": {"htmlLabels": false}} }  %%
graph RL
	%% this is a comment
    %%
    A%%
    
    %%
    -- Text1 ---
    A%%
    
    D""
    `` x==x E --> A%%
    
    A%% -->|Text2|D""
`````

# Edge and Shape

````mermaid
graph RL
    - -.-x D
    
    A ==o xD
    A-o--xD   
    
    Ax--xD
    Ax--text0
    -->
    A --"text1"
    text2
    --> E 
    
    D --> E{"`E{构建理想挠
    	曲线**模型**}`"}
    
    D[
    D:lonely
    ]
        
    : <-- to --> E ~~~~ F
`````



# Subgraph

```mermaid
---
title: 带文本的节点
---
	
 graph LR   
    C
    --> 
    D[[D估算载荷分布]]    
    
    C --> F[F遍历激光雷达数据点]
    F --> G{G是否位于A/B段?}
    
    subgraph "实际扰度
              [计算]" 'calc'
       direction
       G -- 是 --> H[H计算对应位置的载荷 P]
             
       subgraph ad [" "]
        direction TB
       	G x-- LINK-=TEXT --> F
       end       
    end
```

# AMP

```````mermaid
graph BT
    &A & B --> C
    C-->D
```````

# Style

```mermaid
graph LR
	classDef className fill:#f9f,stroke:#333,stroke-width:4px;
	class A className;
	style A-B fill:#bbf,stroke:#f66,stroke-width:2px,color:#fff,stroke-dasharray: 5 5
	A-B-->B@{shape: rounded, label: 'A: 文件,处理'}
```

