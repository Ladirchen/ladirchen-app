export default {
  common: {
    appName: 'Ladirchen',
    familyWorldAria: 'Ladirchen Familienwelt',
    mainNavigation: 'Hauptnavigation',
    save: 'Speichern',
    add: 'Hinzufügen',
    cancel: 'Abbrechen',
    delete: 'Löschen',
    title: 'Titel',
    description: 'Beschreibung',
    close: 'Schließen',
    reset: 'Zurücksetzen',
    next: 'Weiter',
    understood: 'Verstanden',
  },
  onboarding: {
    intro: {
      loadingAria: 'Ladirchen wird geladen',
      message: 'Super-Ladi bringt die Familienwelt in Ordnung …',
    },
  },
  familyPets: {
    animatedAria: '{name}, ein animiertes Haustier der Art {kind}',
    kinds: {
      bird: 'Vogel',
      cat: 'Katze',
      dog: 'Hund',
      other: 'Anderes Tier',
      rabbit: 'Kaninchen',
    },
  },
  family: {
    hero: {
      eyebrow: 'Unsere Familienwelt',
      title: 'Familie',
      description: 'Gemeinsam helfen, sparen und euer Zuhause wachsen lassen.',
      membersAria: 'Mitglieder eurer Familie',
    },
    admin: { title: 'Familie verwalten', description: 'Kinder, Haustiere und Bezugspersonen bearbeiten.', edit: 'Bearbeiten' },
    roles: { guardian: 'Bezugsperson', child: 'Kind' },
    roster: {
      eyebrow: 'Wir gehören zusammen',
      title: 'Unsere Familie',
      count: '1 Mitglied in eurer Familienwelt | {count} Mitglieder in eurer Familienwelt',
      invite: 'Einladen',
      invited: 'Eingeladen',
      noGoal: 'Noch kein sichtbares Ziel',
    },
    permissions: { label: 'Berechtigung', supporter: 'Zielbegleitung', admin: 'Administration' },
    weekly: {
      participate: 'Am Wochenziel teilnehmen',
      active: 'Spielt aktiv mit',
      inactive: 'Zählt nicht zum Familienfortschritt',
      familyProgress: 'Wochenziel-Fortschritt',
      personalProgress: 'Persönliches Wochenziel',
    },
    pets: { eyebrow: 'Pfoten gehören dazu', title: 'Unsere Haustiere', description: 'Auch sie sind Teil eurer Familienwelt.' },
    invite: {
      title: 'Bezugsperson einladen',
      description: 'Lege fest, welche Bereiche die eingeladene Person verwenden darf.',
      email: 'E-Mail-Adresse',
      notice: 'Zielbegleitung sieht nur öffentliche Kinderziele und kann diese unterstützen. Administration erhält die vollständige Verwaltung.',
      submit: 'Einladung vormerken',
    },
    guide: { title: 'Meine Familie', message: 'Hier siehst du Beiträge, Tagesserien und sichtbare Ziele deiner ganzen Familie auf einen Blick.' },
    setup: {
      editEyebrow: 'Familie bearbeiten',
      welcomeEyebrow: 'Willkommen bei Ladirchen',
      title: 'Wer gehört zu eurer Familie?',
      close: 'Einrichtung schließen',
      name: 'Name',
      back: 'Zurück',
      start: 'Familienwelt starten',
      children: {
        title: 'Kinder', description: 'Für jedes Kind entstehen eigene Beiträge, Ladirchen und Sparziele.', name: 'Name des Kindes', remove: 'Kind entfernen', add: 'Kind hinzufügen',
      },
      pets: {
        title: 'Haustiere', description: 'Haustiere können später eigene Familienbeiträge bekommen.', kind: 'Tierart', remove: 'Haustier entfernen', add: 'Haustier hinzufügen',
      },
      guardians: {
        title: 'Bezugspersonen', description: 'Bezugspersonen können Beiträge anlegen, prüfen und die Familienwelt begleiten.', name: 'Name der Bezugsperson', remove: 'Bezugsperson entfernen', add: 'Bezugsperson hinzufügen',
      },
    },
  },
  ladi: {
    smartName: 'Klug-Ladi',
    mascotAria: '{name}, das Sparfaultier, Bewertung {score} von 5',
    stages: {
      super: {
        name: 'Super-Ladi',
        description: 'Fast perfekte Mitarbeit! Mit Umhang und L-Wappen fliegt Super-Ladi zur nächsten Mission.',
      },
      aurora: {
        name: 'Sonnenbrillen-Ladi',
        description: 'Ab 4,3 Sternen ist Ladi ziemlich stolz und setzt seine coolste Sonnenbrille auf.',
      },
      sun: {
        name: 'Sonnen-Ladi',
        description: 'Ladis gesparte Münze leuchtet jetzt wie eine kleine Sonne.',
      },
      garden: {
        name: 'Blätter-Ladi',
        description: 'Geduld zahlt sich aus: Ladis Ast bekommt seine ersten grünen Blätter.',
      },
      starter: {
        name: 'Start-Ladi',
        description: 'Ladi ist aufgestanden und hält seine erste Ladirchen-Münze wieder ganz fest.',
      },
      idle: {
        name: 'Faul-Ladi',
        description: 'Ladi hängt gelangweilt am Ast. Ein paar erledigte Beiträge muntern ihn wieder auf.',
      },
    },
  },
} as const;
