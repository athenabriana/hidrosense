# Test runner image: Maven + JDK 21 with the suite's dependencies baked in.
FROM maven:3.9-eclipse-temurin-21

WORKDIR /tests

# Dependencies first, so changing a feature or step does not re-download them.
COPY pom.xml .
RUN mvn -B -q dependency:go-offline

COPY src ./src

# Extra arguments are appended, e.g. `docker compose run --rm bdd-tests -Dcucumber.filter.tags=@esg`.
ENTRYPOINT ["mvn", "-B", "test"]
