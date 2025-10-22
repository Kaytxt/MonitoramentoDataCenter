# Sistema de Monitoramento de Servidor (Padrão Observer)

Projeto desenvolvido em **C# (.NET)** para demonstrar o uso do **padrão de projeto Observer**, simulando um sistema de monitoramento de servidores.

Quando o servidor tem suas métricas alteradas (como CPU, memória e temperatura), todos os observadores são notificados automaticamente e reagem de acordo com a situação.

---

## Objetivo

Mostrar de forma prática como funciona o padrão **Observer**, onde um objeto principal (o servidor) avisa automaticamente os outros objetos (os observadores) quando há alguma mudança.

---

## Como funciona

- O servidor (`ServidorMonitorado`) gera métricas de forma simulada.  
- Cada observador (como `EquipeInfraestrutura`, `CentroOperacoesNOC`, `SistemaEmail`, etc.) é registrado no servidor.  
- Sempre que o servidor atualiza suas métricas, todos os observadores recebem a notificação e executam uma ação.

---
