package br.com.fiap.hidrosense.bdd.support;

/**
 * Settings read from environment variables, so no credential lives in the repository.
 * Same names as the API's .env, which lets the suite reuse the file that started the stack.
 */
public final class TestConfig {

    private TestConfig() {
    }

    public static String baseUrl() {
        return env("API_BASE_URL", "http://localhost:8080");
    }

    public static String passwordFor(String username) {
        return switch (username) {
            case "admin" -> required("SEED_ADMIN_PASSWORD");
            case "operator" -> required("SEED_OPERATOR_PASSWORD");
            default -> throw new IllegalArgumentException("Usuário sem senha configurada: " + username);
        };
    }

    private static String env(String name, String fallback) {
        String value = System.getenv(name);
        return value == null || value.isBlank() ? fallback : value;
    }

    private static String required(String name) {
        String value = System.getenv(name);
        if (value == null || value.isBlank()) {
            throw new IllegalStateException("Defina a variável de ambiente " + name + " antes de rodar os testes.");
        }
        return value;
    }
}
