export default {
  common: {
    appName: "Ladirchen",
    familyWorldAria: "Ladirchen family world",
    mainNavigation: "Main navigation",
    save: "Save",
    add: "Add",
    cancel: "Cancel",
    delete: "Delete",
    title: "Title",
    description: "Description",
    close: "Close",
    reset: "Reset",
    next: "Next",
    understood: "Got it",
  },
  onboarding: {
    intro: {
      loadingAria: "Ladirchen is loading",
      message: "Super Ladi is getting the family world in order …",
    },
  },
  familyPets: {
    animatedAria: "{name}, an animated {kind}",
    kinds: {
      bird: "Bird",
      cat: "Cat",
      dog: "Dog",
      other: "Other pet",
      rabbit: "Rabbit",
    },
  },
  family: {
    hero: {
      eyebrow: "Our family world",
      title: "Family",
      description: "Help and save together while growing your home.",
      membersAria: "Members of your family",
    },
    admin: { title: "Manage family", description: "Edit children, pets, and caregivers.", edit: "Edit" },
    roles: { guardian: "Caregiver", child: "Child" },
    roster: {
      eyebrow: "We belong together",
      title: "Our family",
      count: "1 member in your family world | {count} members in your family world",
      invite: "Invite",
      invited: "Invited",
      noGoal: "No visible goal yet",
    },
    permissions: { label: "Permission", supporter: "Goal supporter", admin: "Administration" },
    weekly: {
      participate: "Join the weekly goal",
      active: "Participates actively",
      inactive: "Does not count toward family progress",
      familyProgress: "Weekly goal progress",
      personalProgress: "Personal weekly goal",
    },
    pets: { eyebrow: "Paws belong too", title: "Our pets", description: "They are part of your family world too." },
    invite: {
      title: "Invite a caregiver",
      description: "Choose which areas the invited person may use.",
      email: "Email address",
      notice: "Goal supporters can only see and support shared children’s goals. Administrators receive full management access.",
      submit: "Prepare invitation",
    },
    guide: { title: "My family", message: "Here you can see contributions, daily streaks, and visible goals for your entire family at a glance." },
    setup: {
      editEyebrow: "Edit family",
      welcomeEyebrow: "Welcome to Ladirchen",
      title: "Who belongs to your family?",
      close: "Close setup",
      name: "Name",
      back: "Back",
      start: "Start family world",
      children: {
        title: "Children", description: "Each child gets their own contributions, Ladirchen, and saving goals.", name: "Child’s name", remove: "Remove child", add: "Add child",
      },
      pets: {
        title: "Pets", description: "Pets can receive their own family contributions later.", kind: "Pet type", remove: "Remove pet", add: "Add pet",
      },
      guardians: {
        title: "Caregivers", description: "Caregivers can create and review contributions and support the family world.", name: "Caregiver’s name", remove: "Remove caregiver", add: "Add caregiver",
      },
    },
  },
  ladi: {
    smartName: "Smart Ladi",
    mascotAria: "{name}, the saving sloth, rating {score} out of 5",
    stages: {
      super: {
        name: "Super Ladi",
        description: "Almost perfect teamwork! Wearing a cape and L badge, Super Ladi flies to the next mission.",
      },
      aurora: {
        name: "Sunglasses Ladi",
        description: "From 4.3 stars, Ladi is especially proud and puts on the coolest sunglasses.",
      },
      sun: {
        name: "Sun Ladi",
        description: "Ladi’s saved coin now shines like a little sun.",
      },
      garden: {
        name: "Leaf Ladi",
        description: "Patience pays off: the first green leaves are growing on Ladi’s branch.",
      },
      starter: {
        name: "Starter Ladi",
        description: "Ladi is up and proudly holding the first Ladirchen coin again.",
      },
      idle: {
        name: "Resting Ladi",
        description: "Ladi is hanging out on the branch. A few completed contributions will cheer Ladi up again.",
      },
    },
  },
} as const;
