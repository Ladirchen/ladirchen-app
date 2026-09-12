export type LadiTier = 'spark' | 'garden' | 'sun' | 'aurora' | 'super';

export interface LadiStage {
  tier: LadiTier;
  name: string;
  description: string;
}

export const getLadiStage = (score: number): LadiStage => {
  if (score >= 4.8) {
    return {
      tier: 'super',
      name: 'Super-Ladi',
      description: 'Fast perfekte Mitarbeit! Mit Umhang und L-Wappen fliegt Super-Ladi zur nächsten Mission.',
    };
  }

  if (score >= 4.3) {
    return {
      tier: 'aurora',
      name: 'Sonnenbrillen-Ladi',
      description: 'Ab 4,3 Sternen ist Ladi ziemlich stolz und setzt seine coolste Sonnenbrille auf.',
    };
  }

  if (score >= 4) {
    return {
      tier: 'sun',
      name: 'Sonnen-Ladi',
      description: 'Ladis gesparte Münze leuchtet jetzt wie eine kleine Sonne.',
    };
  }

  if (score >= 3) {
    return {
      tier: 'garden',
      name: 'Blätter-Ladi',
      description: 'Geduld zahlt sich aus: Ladis Ast bekommt seine ersten grünen Blätter.',
    };
  }

  if (score >= 2.5) {
    return {
      tier: 'spark',
      name: 'Start-Ladi',
      description: 'Ladi ist aufgestanden und hält seine erste Ladirchen-Münze wieder ganz fest.',
    };
  }

  return {
    tier: 'spark',
    name: 'Faul-Ladi',
    description: 'Ladi hängt gelangweilt am Ast. Ein paar erledigte Beiträge muntern ihn wieder auf.',
  };
};
