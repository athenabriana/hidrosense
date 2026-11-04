package br.com.fiap.hidrosense.bdd.steps;

import br.com.fiap.hidrosense.bdd.support.ApiContext;
import br.com.fiap.hidrosense.bdd.support.Payloads;
import br.com.fiap.hidrosense.bdd.support.TestConfig;
import io.cucumber.java.pt.Quando;

public class AuthSteps {

    private final ApiContext ctx;

    public AuthSteps(ApiContext ctx) {
        this.ctx = ctx;
    }

    @Quando("faço login como {string} com a senha correta")
    public void facoLoginComSenhaCorreta(String username) {
        login(username, TestConfig.passwordFor(username));
    }

    @Quando("faço login como {string} com a senha {string}")
    public void facoLoginComSenha(String username, String password) {
        login(username, password);
    }

    private void login(String username, String password) {
        ctx.clearAuthentication();
        ctx.setResponse(ctx.request()
                .body(Payloads.login(username, password))
                .post("/api/v1/auth/login"));
    }
}
