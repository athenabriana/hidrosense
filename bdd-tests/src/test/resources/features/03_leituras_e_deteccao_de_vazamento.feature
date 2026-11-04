# language: pt
@api @consumption @esg @ambiental
Funcionalidade: Leituras de consumo e detecção de vazamentos
  Para reduzir o desperdício de água (pilar ambiental do ESG)
  Como sistema de telemetria dos hidrômetros
  Quero registrar as leituras de consumo e gerar um alerta quando a vazão indicar vazamento

  Contexto:
    Dado que estou autenticado como "operator"
    E que existe uma unidade consumidora com vazão ociosa de 2.0 litros por minuto

  @positivo
  Cenário: Leitura dentro da vazão esperada não gera alerta
    Quando o hidrômetro envia uma leitura de 150 litros com vazão de 1.5 litros por minuto
    Então o status da resposta deve ser 201
    E a resposta deve obedecer ao contrato "reading-result.schema.json"
    E o campo "leakDetected" deve ser falso
    E o campo "leakAlertId" deve ser nulo

  @positivo
  Esquema do Cenário: Vazão acima da vazão ociosa gera alerta com severidade proporcional
    Quando o hidrômetro envia uma leitura de 300 litros com vazão de <vazao> litros por minuto
    Então o status da resposta deve ser 201
    E a resposta deve obedecer ao contrato "reading-result.schema.json"
    E o campo "leakDetected" deve ser verdadeiro
    E o campo "leakSeverity" deve ser "<severidade>"

    Exemplos: vazão em relação à vazão ociosa de 2.0 L/min
      | vazao | severidade |
      | 2.5   | Low        |
      | 4.0   | Medium     |
      | 6.0   | High       |
      | 10.0  | Critical   |

  @positivo
  Cenário: Histórico de leituras da unidade
    Dado que a unidade já recebeu uma leitura de 120 litros com vazão de 0.5 litros por minuto
    Quando consulto as leituras da unidade
    Então o status da resposta deve ser 200
    E a resposta deve obedecer ao contrato "reading-page.schema.json"
    E o campo "totalItems" deve valer 1
    E todas as leituras devem ser da unidade consultada

  @negativo
  Cenário: Leitura para unidade inexistente é recusada
    Quando o hidrômetro envia uma leitura para a unidade 999999
    Então o status da resposta deve ser 404
    E a resposta deve obedecer ao contrato "problem.schema.json"
    E o campo "detail" deve conter "was not found"

  @negativo
  Cenário: Leitura com volume negativo é rejeitada
    Quando o hidrômetro envia uma leitura de -10 litros com vazão de 1.0 litros por minuto
    Então o status da resposta deve ser 400
    E a resposta deve obedecer ao contrato "validation-problem.schema.json"
    E o erro de validação deve apontar o campo "VolumeLiters"

  @negativo @governanca
  Cenário: Envio de leitura sem token é bloqueado
    Dado que não estou autenticado
    Quando o hidrômetro envia uma leitura de 150 litros com vazão de 1.5 litros por minuto
    Então o status da resposta deve ser 401
