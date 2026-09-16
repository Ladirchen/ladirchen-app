import { AVATAR_FACE_CATALOG, AVATAR_FACE_SHAPE_CATALOG } from '@/domain/avatar';
import { defineAvatarOptions } from './types';

export const faceOptions = defineAvatarOptions('face', AVATAR_FACE_CATALOG);
export const faceShapeOptions = defineAvatarOptions('face-shape', AVATAR_FACE_SHAPE_CATALOG);
