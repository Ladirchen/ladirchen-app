export type LadiTier = 'spark' | 'garden' | 'sun' | 'aurora' | 'super';
export type LadiStageId = 'idle-ladi' | 'starter-ladi' | 'garden-ladi' | 'sun-ladi' | 'aurora-ladi' | 'super-ladi';

export interface LadiStage {
  readonly id: LadiStageId;
  readonly tier: LadiTier;
  readonly minimumScore: number;
  readonly name: string;
  readonly description: string;
}

export const LADI_STAGES: readonly LadiStage[] = [
  {
    id: 'super-ladi',
    tier: 'super',
    minimumScore: 4.8,
    name: 'Super-Ladi',
    description: 'Fast perfekte Mitarbeit! Mit Umhang und L-Wappen fliegt Super-Ladi zur nächsten Mission.',
  },
  {
    id: 'aurora-ladi',
    tier: 'aurora',
    minimumScore: 4.3,
    name: 'Sonnenbrillen-Ladi',
    description: 'Ab 4,3 Sternen ist Ladi ziemlich stolz und setzt seine coolste Sonnenbrille auf.',
  },
  {
    id: 'sun-ladi',
    tier: 'sun',
    minimumScore: 4,
    name: 'Sonnen-Ladi',
    description: 'Ladis gesparte Münze leuchtet jetzt wie eine kleine Sonne.',
  },
  {
    id: 'garden-ladi',
    tier: 'garden',
    minimumScore: 3,
    name: 'Blätter-Ladi',
    description: 'Geduld zahlt sich aus: Ladis Ast bekommt seine ersten grünen Blätter.',
  },
  {
    id: 'starter-ladi',
    tier: 'spark',
    minimumScore: 2.5,
    name: 'Start-Ladi',
    description: 'Ladi ist aufgestanden und hält seine erste Ladirchen-Münze wieder ganz fest.',
  },
  {
    id: 'idle-ladi',
    tier: 'spark',
    minimumScore: Number.NEGATIVE_INFINITY,
    name: 'Faul-Ladi',
    description: 'Ladi hängt gelangweilt am Ast. Ein paar erledigte Beiträge muntern ihn wieder auf.',
  },
];

export const getLadiStage = (score: number): LadiStage =>
  LADI_STAGES.find(stage => score >= stage.minimumScore) ?? LADI_STAGES[LADI_STAGES.length - 1]!;
