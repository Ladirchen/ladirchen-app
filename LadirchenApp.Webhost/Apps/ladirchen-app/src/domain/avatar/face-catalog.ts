import { defineAvatarPart } from "./catalog";

const BUILT_IN_AVATAR_FACE_CATALOG = [
  defineAvatarPart("happy"),
  defineAvatarPart("freckles"),
  defineAvatarPart("wink"),
  defineAvatarPart("surprised"),
  defineAvatarPart("confident"),
  defineAvatarPart("dreamy"),
  defineAvatarPart("silly"),
  defineAvatarPart("sparkle"),
] as const;

export const AVATAR_FACE_EXTENSION_CATALOG = [
  // design-generator:avatar-face-id
] as const;

export const AVATAR_FACE_CATALOG = [
  ...BUILT_IN_AVATAR_FACE_CATALOG,
  ...AVATAR_FACE_EXTENSION_CATALOG,
] as const;

export type AvatarFace = typeof AVATAR_FACE_CATALOG[number]["id"];
export type AvatarFaceExtensionId = typeof AVATAR_FACE_EXTENSION_CATALOG[number]["id"];

const BUILT_IN_AVATAR_FACE_SHAPE_CATALOG = [
  defineAvatarPart("soft"),
  defineAvatarPart("round"),
  defineAvatarPart("oval"),
  defineAvatarPart("angular"),
] as const;

export const AVATAR_FACE_SHAPE_EXTENSION_CATALOG = [
  // design-generator:avatar-face-shape-id
] as const;

export const AVATAR_FACE_SHAPE_CATALOG = [
  ...BUILT_IN_AVATAR_FACE_SHAPE_CATALOG,
  ...AVATAR_FACE_SHAPE_EXTENSION_CATALOG,
] as const;

export type AvatarFaceShape = typeof AVATAR_FACE_SHAPE_CATALOG[number]["id"];
export type AvatarFaceShapeExtensionId = typeof AVATAR_FACE_SHAPE_EXTENSION_CATALOG[number]["id"];
