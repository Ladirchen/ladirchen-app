import { describe, expect, it } from "vite-plus/test";

import { accountActivationInputIsValid, bootstrapRegistrationInputIsValid } from "./auth-config";
import { isEmailAddressInput } from "@/shared/validation/input-validation";

const validActivation = {
  activationCode: "one-time-code",
  email: "member@example.test",
  password: "secret-password",
  passwordConfirmation: "secret-password",
  username: "member",
};

describe("authentication form validation", () => {
  it("accepts an email address for a personal account", () => {
    expect(isEmailAddressInput("member@example.test")).toBe(true);
    expect(isEmailAddressInput("not-an-email")).toBe(false);
  });

  it("requires complete bootstrap registration credentials", () => {
    expect(
      bootstrapRegistrationInputIsValid({
        email: "guardian@example.test",
        familyName: "Example family",
        guardianName: "Alex",
        password: "secret-password",
        passwordConfirmation: "secret-password",
        username: "alex",
      }),
    ).toBe(true);
    expect(
      bootstrapRegistrationInputIsValid({
        email: "guardian@example.test",
        familyName: "Example family",
        guardianName: "Alex",
        password: "secret-password",
        passwordConfirmation: "different-password",
        username: "alex",
      }),
    ).toBe(false);
  });

  it("requires the one-time code before activation", () => {
    expect(
      accountActivationInputIsValid({
        activationCode: "",
        email: "member@example.test",
        password: "secret-password",
        passwordConfirmation: "secret-password",
        username: "member",
      }),
    ).toBe(false);
    expect(
      accountActivationInputIsValid({
        activationCode: "one-time-code",
        email: "member@example.test",
        password: "secret-password",
        passwordConfirmation: "secret-password",
        username: "member",
      }),
    ).toBe(true);
  });

  it("requires at least twelve characters for a new password", () => {
    expect(
      accountActivationInputIsValid({
        activationCode: "one-time-code",
        email: "member@example.test",
        password: "12345678901",
        passwordConfirmation: "12345678901",
        username: "member",
      }),
    ).toBe(false);
    expect(
      accountActivationInputIsValid({
        activationCode: "one-time-code",
        email: "member@example.test",
        password: "123456789012",
        passwordConfirmation: "123456789012",
        username: "member",
      }),
    ).toBe(true);
  });

  it("applies the backend username format and length boundaries", () => {
    expect(accountActivationInputIsValid({ ...validActivation, username: "member name" })).toBe(false);
    expect(accountActivationInputIsValid({ ...validActivation, username: "member@example.test" })).toBe(false);
    expect(accountActivationInputIsValid({ ...validActivation, username: "a".repeat(100) })).toBe(true);
    expect(accountActivationInputIsValid({ ...validActivation, username: "a".repeat(101) })).toBe(false);
  });

  it("applies the backend password and email maximum lengths", () => {
    const maximumPassword = "p".repeat(256);

    expect(
      accountActivationInputIsValid({
        ...validActivation,
        password: maximumPassword,
        passwordConfirmation: maximumPassword,
      }),
    ).toBe(true);
    expect(
      accountActivationInputIsValid({
        ...validActivation,
        password: `${maximumPassword}p`,
        passwordConfirmation: `${maximumPassword}p`,
      }),
    ).toBe(false);
    expect(accountActivationInputIsValid({ ...validActivation, email: `${"a".repeat(309)}@example.test` })).toBe(false);
  });

  it("rejects a whitespace-only password", () => {
    expect(
      accountActivationInputIsValid({
        ...validActivation,
        password: " ".repeat(12),
        passwordConfirmation: " ".repeat(12),
      }),
    ).toBe(false);
  });
});
