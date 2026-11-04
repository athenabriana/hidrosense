package br.com.fiap.hidrosense.bdd.support;

import io.restassured.RestAssured;
import io.restassured.http.ContentType;
import io.restassured.response.Response;
import io.restassured.specification.RequestSpecification;

import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

import static org.hamcrest.Matchers.equalTo;

/**
 * Per-scenario state shared by the step classes (injected by cucumber-picocontainer).
 * Fixture calls (creating units, readings, alerts) go through {@link #asAdmin()} and never
 * overwrite {@link #response}, which always holds the response under test.
 */
public class ApiContext {

    // Tokens are reused across scenarios: logging in once per role keeps the run fast.
    private static final Map<String, String> TOKENS = new ConcurrentHashMap<>();

    private String token;
    private Response response;

    private Integer unitId;
    private String unitCode;
    private String unitRegion;
    private Long alertId;

    static {
        RestAssured.baseURI = TestConfig.baseUrl();
        RestAssured.enableLoggingOfRequestAndResponseIfValidationFails();
    }

    public RequestSpecification request() {
        RequestSpecification spec = RestAssured.given().contentType(ContentType.JSON).accept(ContentType.JSON);
        return token == null ? spec : spec.auth().oauth2(token);
    }

    public RequestSpecification asAdmin() {
        return RestAssured.given().contentType(ContentType.JSON).auth().oauth2(tokenFor("admin"));
    }

    public void authenticateAs(String username) {
        token = tokenFor(username);
    }

    public void clearAuthentication() {
        token = null;
    }

    public static String tokenFor(String username) {
        return TOKENS.computeIfAbsent(username, u -> RestAssured.given()
                .contentType(ContentType.JSON)
                .body(Map.of("username", u, "password", TestConfig.passwordFor(u)))
                .post("/api/v1/auth/login")
                .then().statusCode(200)
                .extract().path("accessToken"));
    }

    /** Creates an active consumer unit with a unique code, so runs never collide on the same database. */
    public void createUnit(String region, double idleFlowThresholdLpm) {
        String code = uniqueCode();
        Response created = asAdmin()
                .body(Payloads.consumerUnit(code, region, idleFlowThresholdLpm))
                .post("/api/v1/consumer-units");
        created.then().statusCode(201);
        unitId = created.path("id");
        unitCode = code;
        unitRegion = region;
    }

    public Response postReading(int consumerUnitId, double volumeLiters, double flowRateLpm) {
        return asAdmin()
                .body(Payloads.reading(consumerUnitId, volumeLiters, flowRateLpm))
                .post("/api/v1/consumption/readings");
    }

    /** Builds an open leak alert through the real detection rule: flow 5x the idle baseline. */
    public void createOpenAlert(String region) {
        createUnit(region, 2.0);
        Response reading = postReading(unitId, 600, 10.0);
        reading.then().statusCode(201).body("leakDetected", equalTo(true));
        alertId = ((Number) reading.path("leakAlertId")).longValue();
    }

    public static String uniqueCode() {
        return "BDD-" + UUID.randomUUID().toString().substring(0, 8).toUpperCase();
    }

    public static String uniqueRegion() {
        return "Região BDD " + UUID.randomUUID().toString().substring(0, 8);
    }

    public Response response() {
        if (response == null) {
            throw new IllegalStateException("Nenhuma requisição foi feita neste cenário.");
        }
        return response;
    }

    public void setResponse(Response response) {
        this.response = response;
    }

    public boolean hasResponse() {
        return response != null;
    }

    public Integer unitId() {
        return unitId;
    }

    public String unitCode() {
        return unitCode;
    }

    public String unitRegion() {
        return unitRegion;
    }

    public Long alertId() {
        return alertId;
    }
}
