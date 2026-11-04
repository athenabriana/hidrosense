# language: pt
@api @leak-alerts @esg @governanca
Funcionalidade: Tratamento de alertas de vazamento
  Para que cada vazamento tenha um responsável e um desfecho registrado (governança)
  Como administrador da companhia de saneamento
  Quero consultar os alertas abertos e registrar a resolução de cada um

  Contexto:
    Dado que existe um alerta de vazamento aberto

  @positivo
  Cenário: Operador consulta os alertas abertos da região
    Dado que estou autenticado como "operator"
    Quando consulto os alertas abertos da região do alerta
    Então o status da resposta deve ser 200
    E a resposta deve obedecer ao contrato "leak-alert-page.schema.json"
    E o campo "totalItems" deve valer 1
    E todos os itens de "items" devem ter "status" igual a "Open"
    E todos os itens de "items" devem ter "severity" igual a "Critical"

  @positivo
  Cenário: Administrador resolve o alerta e fica registrado quem resolveu
    Dado que estou autenticado como "admin"
    Quando resolvo o alerta com a nota "Válvula substituída pela equipe de campo"
    Então o status da resposta deve ser 200
    E a resposta deve obedecer ao contrato "leak-alert.schema.json"
    E o campo "status" deve ser "Resolved"
    E o campo "resolvedBy" deve ser "admin"
    E o campo "message" deve conter "Válvula substituída pela equipe de campo"

  @positivo
  Cenário: Administrador descarta um falso positivo
    Dado que estou autenticado como "admin"
    Quando descarto o alerta como falso positivo
    Então o status da resposta deve ser 200
    E a resposta deve obedecer ao contrato "leak-alert.schema.json"
    E o campo "status" deve ser "Dismissed"

  @negativo
  Cenário: Alerta já resolvido não pode ser resolvido de novo
    Dado que estou autenticado como "admin"
    E que o alerta já foi resolvido
    Quando resolvo o alerta com a nota "Segunda tentativa"
    Então o status da resposta deve ser 409
    E a resposta deve obedecer ao contrato "problem.schema.json"
    E o campo "detail" deve conter "already Resolved"

  @negativo
  Cenário: Alerta inexistente não pode ser resolvido
    Dado que estou autenticado como "admin"
    Quando tento resolver um alerta inexistente
    Então o status da resposta deve ser 404
    E a resposta deve obedecer ao contrato "problem.schema.json"

  @negativo
  Cenário: Operador não tem permissão para resolver alertas
    Dado que estou autenticado como "operator"
    Quando resolvo o alerta com a nota "Tentativa sem permissão"
    Então o status da resposta deve ser 403
