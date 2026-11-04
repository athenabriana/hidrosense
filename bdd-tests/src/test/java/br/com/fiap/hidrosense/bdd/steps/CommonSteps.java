package br.com.fiap.hidrosense.bdd.steps;

import br.com.fiap.hidrosense.bdd.support.ApiContext;
import io.cucumber.java.After;
import io.cucumber.java.Scenario;
import io.cucumber.java.pt.Dado;
import io.cucumber.java.pt.Então;
import io.cucumber.java.pt.Quando;
import io.restassured.RestAssured;
import io.restassured.response.Response;

import java.util.List;
import java.util.Map;

import static io.restassured.module.jsv.JsonSchemaValidator.matchesJsonSchemaInClasspath;
import static org.hamcrest.MatcherAssert.assertThat;
import static org.hamcrest.Matchers.containsString;
import static org.hamcrest.Matchers.equalTo;
import static org.hamcrest.Matchers.everyItem;
import static org.hamcrest.Matchers.is;
import static org.hamcrest.Matchers.notNullValue;
import static org.hamcrest.Matchers.nullValue;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

/** Steps shared by every feature: authentication state, status code, JSON body and contract checks. */
public class CommonSteps {

    private final ApiContext ctx;

    public CommonSteps(ApiContext ctx) {
        this.ctx = ctx;
    }

    @Dado("que estou autenticado como {string}")
    public void queEstouAutenticadoComo(String username) {
        ctx.authenticateAs(username);
    }

    @Dado("que não estou autenticado")
    public void queNaoEstouAutenticado() {
        ctx.clearAuthentication();
    }

    @Quando("consulto {string}")
    public void consulto(String path) {
        ctx.setResponse(ctx.request().get(path));
    }

    @Quando("verifico a saúde da API")
    public void verificoASaudeDaApi() {
        ctx.setResponse(RestAssured.given().get("/health"));
    }

    @Então("o status da resposta deve ser {int}")
    public void oStatusDaRespostaDeveSer(int status) {
        ctx.response().then().statusCode(status);
    }

    @Então("a resposta deve obedecer ao contrato {string}")
    public void aRespostaDeveObedecerAoContrato(String schema) {
        ctx.response().then().body(matchesJsonSchemaInClasspath("schemas/" + schema));
    }

    @Então("a resposta deve ser o texto {string}")
    public void aRespostaDeveSerOTexto(String text) {
        assertEquals(text, ctx.response().asString());
    }

    @Então("o campo {string} deve ser {string}")
    public void oCampoDeveSer(String field, String expected) {
        ctx.response().then().body(field, equalTo(expected));
    }

    @Então("o campo {string} deve conter {string}")
    public void oCampoDeveConter(String field, String expected) {
        ctx.response().then().body(field, containsString(expected));
    }

    @Então("o campo {string} deve valer {double}")
    public void oCampoDeveValer(String field, double expected) {
        Number actual = ctx.response().path(field);
        assertThat("campo " + field, actual, notNullValue());
        assertEquals(expected, actual.doubleValue(), 0.001, "campo " + field);
    }

    @Então("o campo {string} deve ser verdadeiro")
    public void oCampoDeveSerVerdadeiro(String field) {
        ctx.response().then().body(field, is(true));
    }

    @Então("o campo {string} deve ser falso")
    public void oCampoDeveSerFalso(String field) {
        ctx.response().then().body(field, is(false));
    }

    @Então("o campo {string} deve ser nulo")
    public void oCampoDeveSerNulo(String field) {
        ctx.response().then().body(field, nullValue());
    }

    @Então("a lista {string} não deve estar vazia")
    public void aListaNaoDeveEstarVazia(String field) {
        List<?> items = ctx.response().path(field);
        assertThat("lista " + field, items, notNullValue());
        assertThat("lista " + field, items.isEmpty(), is(false));
    }

    @Então("todos os itens de {string} devem ter {string} igual a {string}")
    public void todosOsItensDevemTer(String list, String field, String expected) {
        aListaNaoDeveEstarVazia(list);
        ctx.response().then().body(list + "." + field, everyItem(equalTo(expected)));
    }

    /** ASP.NET reports validation errors keyed by the C# property name (PascalCase). */
    @Então("o erro de validação deve apontar o campo {string}")
    public void oErroDeValidacaoDeveApontarOCampo(String field) {
        Map<String, List<String>> errors = ctx.response().path("errors");
        assertThat("errors", errors, notNullValue());
        boolean found = errors.keySet().stream().anyMatch(k -> k.equalsIgnoreCase(field));
        assertTrue(found, "esperava erro de validação em '" + field + "', recebido: " + errors.keySet());
    }

    /** Attaches the last response to the Cucumber HTML report, the evidence of each scenario. */
    @After
    public void anexarResposta(Scenario scenario) {
        if (!ctx.hasResponse()) {
            return;
        }
        Response response = ctx.response();
        // Reports are published as CI artifacts: keep bearer tokens out of them.
        String body = response.asPrettyString()
                .replaceAll("(\"accessToken\"\\s*:\\s*\")[^\"]+\"", "$1<omitido>\"");
        String evidence = "HTTP " + response.statusCode() + System.lineSeparator() + body;
        scenario.attach(evidence, "text/plain", "Resposta da API");
    }
}
