import { EMAIL_INPUT_MAXIMUM_LENGTH, isEmailAddressInput } from "@/shared/validation/input-validation";

export const AUTH_INPUT_RULES = Object.freeze({
  emailMaximumLength: EMAIL_INPUT_MAXIMUM_LENGTH,
  familyNameMinimumLength: 2,
  guardianNameMinimumLength: 2,
  passwordMaximumLength: 256,
  passwordMinimumLength: 12,
  usernameMaximumLength: 100,
  usernameMinimumLength: 3,
});

export interface AccountActivationInput {
  readonly activationCode: string;
  readonly email: string;
  readonly password: string;
  readonly passwordConfirmation: string;
  readonly username: string;
}

export interface BootstrapRegistrationInput {
  readonly email: string;
  readonly familyName: string;
  readonly guardianName: string;
  readonly password: string;
  readonly passwordConfirmation: string;
  readonly username: string;
}

function credentialsAreValid(input: {
  readonly password: string;
  readonly passwordConfirmation: string;
  readonly username: string;
}): boolean {
  const normalizedUsername = input.username.trim();

  return (
    normalizedUsername.length >= AUTH_INPUT_RULES.usernameMinimumLength &&
    normalizedUsername.length <= AUTH_INPUT_RULES.usernameMaximumLength &&
    !normalizedUsername.includes("@") &&
    !/\s/u.test(normalizedUsername) &&
    input.password.length >= AUTH_INPUT_RULES.passwordMinimumLength &&
    input.password.length <= AUTH_INPUT_RULES.passwordMaximumLength &&
    input.password.trim().length > 0 &&
    input.password === input.passwordConfirmation
  );
}

export function accountActivationInputIsValid(input: AccountActivationInput): boolean {
  return isEmailAddressInput(input.email) && input.activationCode.trim().length > 0 && credentialsAreValid(input);
}

export function bootstrapRegistrationInputIsValid(input: BootstrapRegistrationInput): boolean {
  return (
    input.familyName.trim().length >= AUTH_INPUT_RULES.familyNameMinimumLength &&
    input.guardianName.trim().length >= AUTH_INPUT_RULES.guardianNameMinimumLength &&
    isEmailAddressInput(input.email) &&
    credentialsAreValid(input)
  );
}
