# language: pt
@api @auth @governanca
Funcionalidade: Autenticação na API HidroSense
  Para que toda ação sobre o monitoramento hídrico seja rastreável até quem a executou
  Como usuário da companhia de saneamento
  Quero obter um token JWT que identifique o meu perfil de acesso

  @positivo
  Esquema do Cenário: Login com credenciais válidas emite token do perfil
    Quando faço login como "<usuario>" com a senha correta
    Então o status da resposta deve ser 200
    E a resposta deve obedecer ao contrato "token.schema.json"
    E o campo "tokenType" deve ser "Bearer"
    E o campo "role" deve ser "<perfil>"

    Exemplos:
      | usuario  | perfil   |
      | admin    | Admin    |
      | operator | Operator |

  @negativo
  Cenário: Login com senha incorreta é recusado
    Quando faço login como "admin" com a senha "senha-incorreta"
    Então o status da resposta deve ser 401
    E a resposta deve obedecer ao contrato "problem.schema.json"
    E o campo "detail" deve ser "Invalid username or password."

  @negativo
  Cenário: Login de usuário inexistente é recusado
    Quando faço login como "intruso" com a senha "qualquer-senha"
    Então o status da resposta deve ser 401
    E a resposta deve obedecer ao contrato "problem.schema.json"

  @negativo
  Cenário: Login sem usuário e senha falha na validação
    Quando faço login como "" com a senha ""
    Então o status da resposta deve ser 400
    E a resposta deve obedecer ao contrato "validation-problem.schema.json"
    E o erro de validação deve apontar o campo "Username"
    E o erro de validação deve apontar o campo "Password"
