<template>
  <span class="avatar-figure" :class="[`outfit-${appearance.outfit}`, `age-${appearance.age ?? 'child'}`, { reacting: isReacting, 'full-body': fullBody, calm }]" :style="figureStyle" role="img" :aria-label="t('avatar.figureAria')" @click="react">
    <svg :viewBox="fullBody ? '0 0 160 260' : '0 0 160 180'">
      <template v-if="!fullBody">
        <circle class="backdrop" cx="80" cy="88" r="74" />
        <circle class="backdrop-dot dot-one" cx="25" cy="38" r="7" />
        <circle class="backdrop-dot dot-two" cx="135" cy="115" r="10" />
      </template>

      <component :is="funAccessoryVisual" v-if="funAccessoryVisual" layer="back" />
      <g v-if="appearance.funAccessoryId === 'rainbow'" class="rainbow">
        <path d="M17 111c4-58 121-58 126 0" />
        <path d="M25 111c5-47 105-47 110 0" />
        <path d="M34 111c4-35 88-35 92 0" />
      </g>

      <g v-if="appearance.funAccessoryId === 'monster-horns'" class="monster-horns">
        <path d="M45 38C25 33 21 12 28 3c4 13 16 12 25 23Z" />
        <path d="M115 38c20-5 24-26 17-35-4 13-16 12-25 23Z" />
        <circle cx="31" cy="9" r="4" /><circle cx="129" cy="9" r="4" />
      </g>

      <component :is="hairVisual" v-if="hairVisual" layer="back" />
      <g v-if="appearance.hair === 'ponytail'" class="hair back-hair">
        <path d="M111 52c28-4 37 16 29 34-5 12-15 23-10 37-24-6-29-25-20-44Z" />
        <path class="hair-highlight" d="M122 61q14 12 6 31m-1 8q-2 9 3 15" />
      </g>
      <g v-if="appearance.hair === 'curls'" class="hair back-hair long-curls">
        <path d="M29 58C23 24 48 4 80 5s57 20 51 56l3 57-13-8-8 14-12-11-11 13-12-13-12 13-11-13-12 11-8-14-13 8Z" />
        <path class="curl-highlight" d="M38 48q-10 12 1 23t-1 22m84-45q10 12-1 23t1 22M45 29q10-13 21-7m49 7q-10-13-21-7M47 102q7 9 14 0m38 0q7 9 14 0" />
      </g>
      <g v-if="appearance.hair === 'waves'" class="hair back-hair long-waves">
        <path d="M33 59C29 22 50 7 80 7s51 16 47 54l6 70-15-10-10 12-13-11-14 12-14-12-13 11-10-12-16 10Z" />
        <path class="hair-highlight" d="M43 49q-8 23 2 44t-3 31m76-75q8 23-2 44t3 31M57 23q-10 15-6 28m52-28q10 15 6 28" />
      </g>
      <path v-if="appearance.hair === 'bob'" class="hair back-hair" d="M34 74c-1-39 18-60 47-60 30 0 47 22 45 62l-5 40-16-8-12 10-13-9-13 9-12-10-16 8Z" />
      <g v-if="appearance.hair === 'bun'" class="hair back-hair bun-hair"><path d="M59 19C54 4 68-4 80 3 92-4 106 4 101 19c-8 8-34 8-42 0Z" /><path d="M38 69c-1-35 16-54 43-54 27 0 44 20 41 56l-12 29H51Z" /><path class="hair-highlight" d="M66 13q14-9 28 0" /></g>
      <g v-if="appearance.hair === 'space-buns'" class="hair back-hair space-bun-hair"><circle cx="42" cy="27" r="19" /><circle cx="118" cy="27" r="19" /><path d="M38 68c0-34 17-52 43-52 27 0 43 19 41 54l-12 30H51Z" /><path class="hair-highlight" d="M32 25q10-10 20 0m56 0q10-10 20 0" /></g>
      <g v-if="appearance.hair === 'afro'" class="hair back-hair afro-hair"><path d="M24 75C5 59 16 38 29 33 25 16 45 6 58 12 66-2 89-2 98 11c17-7 34 6 32 22 17 6 22 28 7 40 9 17-6 34-23 31-8 15-29 15-37 4-11 12-32 9-37-5-17 2-29-15-20-29Z" /><path class="hair-highlight" d="M30 45q9-14 21-9m4-17q11-7 21 1m30 0q12 1 16 11m7 20q8 10 2 21" /></g>
      <g v-if="appearance.hair === 'braids'" class="hair back-hair braided-hair"><path d="M38 63c0-33 17-50 43-50 27 0 43 18 42 52l-8 30H47Z" /><path d="M42 77c-12 9-13 20-3 27-10 7-8 20 3 25 10-6 11-18 2-25 10-8 9-19-2-27Zm76 0c12 9 13 20 3 27 10 7 8 20-3 25-10-6-11-18-2-25-10-8-9-19 2-27Z" /><path class="braid-tie" d="M34 125h16m76 0h-16" /></g>

      <component :is="outfitVisual" v-if="outfitVisual" layer="back" />
      <g v-if="appearance.outfit === 'superhero'" class="outfit-back superhero-cape">
        <path d="m57 121-33 53 44-10 12-34 12 34 44 10-33-53Z" />
      </g>
      <g v-else-if="appearance.outfit === 'fairy'" class="outfit-back fairy-wings">
        <path d="M57 129C31 98 8 113 23 143c8 16 24 21 40 16Z" />
        <path d="M103 129c26-31 49-16 34 14-8 16-24 21-40 16Z" />
        <path d="M59 140c-20-9-27-3-29 8m71-8c20-9 27-3 29 8" />
      </g>
      <g v-else-if="appearance.outfit === 'dinosaur'" class="outfit-back dino-spikes">
        <path d="m45 137-15-9 5 18-16 2 14 13-12 10 23 2m71-36 15-9-5 18 16 2-14 13 12 10-23 2" />
      </g>
      <g v-else-if="appearance.outfit === 'monster'" class="outfit-back monster-fur">
        <path d="m45 128-13-10-2 15-15 1 10 12-8 13 16 1 2 16 14-10m66-38 13-10 2 15 15 1-10 12 8 13-16 1-2 16-14-10" />
      </g>
      <g v-else-if="appearance.outfit === 'vampire'" class="outfit-back vampire-cape">
        <path d="M58 119 23 136 8 178l43-13 29 15 29-15 43 13-15-42-35-17-22 15Z" />
        <path class="vampire-cape-lining" d="M58 124 28 143l-9 27 32-10 29 17 29-17 32 10-9-27-30-19-22 14Z" />
      </g>
      <g v-else-if="appearance.outfit === 'shark'" class="outfit-back shark-fins">
        <path class="shark-dorsal-fin" d="M80 125 109 87l2 52Z" />
        <path class="shark-side-fin" d="m47 145-36 17 31 9m71-26 36 17-31 9" />
      </g>

      <g v-if="fullBody" class="full-body-lower">
        <path class="trousers" d="M48 169h64l-3 35-20 1-9-20-9 20-20-1Z" />
        <path class="leg leg-left" d="M65 197 61 239" />
        <path class="leg leg-right" d="m95 197 4 42" />
        <path class="shoe shoe-left" d="M39 245q7-13 24-9l11 9q1 8-9 8H45q-9 0-6-8Z" />
        <path class="shoe shoe-right" d="M121 245q-7-13-24-9l-11 9q-1 8 9 8h20q9 0 6-8Z" />
      </g>
      <path class="torso" d="M27 180c2-45 21-62 53-62s52 17 53 62Z" />
      <g class="arms">
        <g class="arm-left"><path d="M48 139c-14 6-22 20-25 35" /><circle cx="22" cy="174" r="8" /></g>
        <g class="arm-right waving-arm"><path d="M112 139c14 6 22 20 25 35" /><circle cx="138" cy="174" r="8" /></g>
      </g>
      <path class="neck" d="M67 105h26v27H67Z" />
      <circle class="ear" cx="39" cy="77" r="11" />
      <circle class="ear" cx="121" cy="77" r="11" />
      <component :is="faceShapeVisual" v-if="faceShapeVisual" />
      <ellipse v-else-if="appearance.faceShape === 'round'" class="head head-round" cx="80" cy="73" rx="44" ry="44" />
      <ellipse v-else-if="appearance.faceShape === 'oval'" class="head head-oval" cx="80" cy="72" rx="37" ry="51" />
      <path v-else-if="appearance.faceShape === 'angular'" class="head head-angular" d="M80 21c24 0 41 15 41 43v18c0 18-16 36-41 43-25-7-41-25-41-43V64c0-28 17-43 41-43Z" />
      <path v-else class="head head-soft" d="M80 22c25 0 42 16 42 45 0 30-17 54-42 54S38 97 38 67c0-29 17-45 42-45Z" />

      <g class="hair front-hair">
        <component :is="hairVisual" v-if="hairVisual" layer="front" />
        <g v-else-if="appearance.hair === 'short'" class="short-hair"><path d="M39 62c-2-31 15-49 42-49 24 0 39 15 40 42-10-1-18-6-24-17-12 13-31 21-58 24Z" /><path class="hair-highlight" d="M53 43q17-7 29-23m-13 29q19-8 28-23" /></g>
        <g v-else-if="appearance.hair === 'curls'">
          <path d="M37 58C31 34 45 14 65 12c6-11 27-11 33 1 18 2 30 22 24 43-11-2-19-10-22-20-4 13-13 17-21 6-5 12-15 14-22 3-4 8-11 12-20 13Z" />
          <path class="curl-fringe" d="M45 43q8 12 17 1 8 13 18 0 9 13 19 0 8 10 17-1" />
        </g>
        <g v-else-if="appearance.hair === 'ponytail' || appearance.hair === 'waves'" class="side-part-hair"><path d="M38 61c0-30 16-48 43-48 25 0 40 16 41 44-13-1-23-8-30-22-11 14-29 23-54 26Z" /><path class="hair-highlight" d="M54 47q19-8 34-27m10 11q7 12 17 17" /></g>
        <g v-else-if="appearance.hair === 'bob'" class="bob-front"><path d="M38 63c-1-32 15-51 43-51 28 0 43 19 42 52-15-4-25-13-31-28-13 16-31 24-54 27Z" /><path class="hair-highlight" d="M53 48q21-9 35-29m9 12q8 15 19 21" /></g>
        <g v-else-if="appearance.hair === 'bun' || appearance.hair === 'space-buns'" class="smooth-updo"><path d="M38 61c0-30 16-48 43-48 25 0 40 17 41 45-14-1-24-8-32-22-12 14-29 22-52 25Z" /><path class="hair-highlight" d="M55 45q18-9 31-27m10 11q7 14 18 20" /></g>
        <g v-else-if="appearance.hair === 'afro'" class="afro-front"><path d="M34 62C25 44 37 24 53 22 58 7 76 7 81 16c10-12 29-5 31 8 15 5 20 23 12 37-11-1-19-8-24-20-5 9-12 13-20 6-8 7-16 4-20-6-7 11-15 18-26 21Z" /><path class="hair-highlight" d="M45 39q9-10 18-4m9-13q9-5 17 1m14 8q9 3 12 11" /></g>
        <g v-else-if="appearance.hair === 'braids'" class="braid-front"><path d="M38 61c0-31 16-48 43-48 26 0 41 17 41 47-13-3-23-11-29-25-13 15-31 23-55 26Z" /><path class="hair-highlight" d="M54 46q19-8 33-27m10 11q7 14 18 19" /></g>
        <g v-else class="undercut-hair"><path d="M44 54C47 24 65 8 99 14c13 2 20 11 21 25-10-8-20-11-30-10-12 14-27 22-46 25Z" /><path class="hair-highlight" d="M55 43q18-18 42-22m-25 19q15-11 31-12" /><path class="shaved-line" d="M105 39q8 3 13 9m-15-2q8 3 13 9m-15-2q7 3 11 8" /></g>
      </g>

      <component :is="faceVisual" v-if="faceVisual" />
      <g v-else class="face-details">
        <template v-if="appearance.face === 'dreamy'">
          <path class="dreamy-eyes" d="M51 71q9 8 18 0m22 0q9 8 18 0" />
        </template>
        <template v-else-if="appearance.face === 'wink'">
          <path class="eye-line" d="M55 72q7-6 14 0" />
          <ellipse class="eye" cx="101" cy="70" rx="6.5" ry="8.5" />
        </template>
        <template v-else-if="appearance.face === 'silly'">
          <ellipse class="eye" cx="59" cy="70" rx="7.5" ry="9.5" /><ellipse class="eye" cx="101" cy="71" rx="5.5" ry="7.5" />
        </template>
        <template v-else>
          <ellipse class="eye" cx="60" cy="70" rx="6.5" ry="8.5" />
          <ellipse class="eye" cx="100" cy="70" rx="6.5" ry="8.5" />
        </template>
        <template v-if="appearance.face !== 'dreamy'">
          <circle v-if="appearance.face !== 'wink'" class="eye-glint" cx="62" cy="67" r="2.2" /><circle class="eye-glint" cx="102" cy="67" r="2.2" />
        </template>
        <path v-if="appearance.face === 'confident'" class="eyebrows confident-brows" d="M50 59q10-8 19-1m23 0q9-3 18 2" />
        <path v-else class="eyebrows" d="M50 58q10-6 19 0m22 0q10-6 19 0" />
        <g v-if="appearance.face === 'sparkle'" class="lashes"><path d="m52 63-5-5m10 3-2-7m53 9 5-5m-10 3 2-7" /><circle cx="57" cy="72" r="2" /><circle cx="97" cy="72" r="2" /></g>
        <path class="tiny-nose" d="m80 73-3 9 6 1" />
        <circle class="blush" cx="49" cy="88" r="5" /><circle class="blush" cx="111" cy="88" r="5" />
        <path v-if="appearance.face === 'surprised'" class="mouth surprised" d="M74 91q6-8 12 0-1 11-6 11t-6-11Z" />
        <g v-else-if="appearance.face === 'silly'" class="silly-mouth"><path d="M68 91q12 13 24 0-1 18-12 18T68 91Z" /><path d="M74 103q6-6 12 0" /></g>
        <path v-else-if="appearance.face === 'confident'" class="mouth confident-mouth" d="M68 94q13 7 25-3" />
        <path v-else class="mouth" d="M69 91q11 11 22 0" />
        <g v-if="appearance.face === 'freckles'" class="freckles">
          <circle cx="53" cy="84" r="1.5" /><circle cx="58" cy="86" r="1.3" /><circle cx="63" cy="84" r="1.4" />
          <circle cx="97" cy="84" r="1.4" /><circle cx="102" cy="86" r="1.3" /><circle cx="107" cy="84" r="1.5" />
        </g>
        <g v-if="appearance.age === 'senior'" class="senior-details">
          <path d="M48 78q7 4 13 0m38 0q7 4 13 0M54 99q5 4 10 1m32 0q5 3 10-1" />
          <path class="smile-line" d="M68 106q12 6 24 0" />
        </g>
      </g>

      <g v-if="appearance.funAccessoryId === 'whiskers'" class="whiskers">
        <path d="M53 91 31 86m23 10-22 4m75-9 22-5m-23 10 22 4" />
      </g>
      <path v-if="appearance.funAccessoryId === 'mustache'" class="mustache" d="M80 90c-7-10-17-7-18 1 1 9 13 9 18 2 5 7 17 7 18-2-1-8-11-11-18-1Z" />
      <g v-if="appearance.funAccessoryId === 'clown-nose'" class="clown-nose">
        <circle cx="80" cy="82" r="10" /><ellipse cx="76" cy="78" rx="3" ry="2" />
      </g>
      <component :is="funAccessoryVisual" v-if="funAccessoryVisual" layer="front" />

      <g v-if="appearance.funAccessoryId === 'pirate'" class="pirate-patch">
        <path d="M36 50c30 8 59 8 88 0" />
        <path d="M49 61q12-8 24 0l-2 22q-11 10-22 0Z" />
        <path class="pirate-star" d="m60 65 2 5 5 1-4 3 1 5-4-3-5 3 2-5-4-3 5-1Z" />
      </g>

      <component :is="outfitVisual" v-if="outfitVisual && showsOutfitHeadwear" layer="head" />
      <g v-else-if="appearance.outfit === 'superhero' && showsOutfitHeadwear" class="costume-head superhero-mask">
        <path d="M43 63q17-15 37 0 20-15 37 0l-8 22q-14 5-29-5-15 10-29 5Z" />
        <ellipse cx="60" cy="70" rx="7" ry="5" /><ellipse cx="100" cy="70" rx="7" ry="5" />
        <path class="hero-mask-flare" d="m44 65-12-7 7 15m77-8 12-7-7 15" />
      </g>
      <g v-else-if="appearance.outfit === 'dinosaur' && showsOutfitHeadwear" class="costume-head dino-hood">
        <path class="dino-cap" d="M34 62C29 24 48 7 80 7s51 17 46 55l-13-13-9 10-12-12-12 12-12-12-12 12-9-10Z" />
        <path class="dino-spine" d="m52 26 5-18 12 14L80 2l11 20 12-14 5 19" />
        <path class="dino-tooth" d="m44 55 8 12 7-13 8 12 7-13 8 12 8-12 7 13 8-12 7 13 7-12" />
        <circle class="dino-hood-eye" cx="58" cy="34" r="5" /><circle class="dino-hood-eye" cx="102" cy="34" r="5" />
      </g>
      <g v-else-if="appearance.outfit === 'monster' && showsOutfitHeadwear" class="costume-head monster-hood">
        <path class="monster-cap" d="M30 70C19 35 38 5 80 5s61 30 50 65l-13-17-8 16-12-15-17 13-17-13-12 15-8-16Z" />
        <path class="monster-horn" d="M44 28 28 5l29 12m59 11 16-23-29 12" />
        <circle class="monster-hood-eye" cx="80" cy="27" r="10" /><circle class="monster-hood-pupil" cx="80" cy="27" r="4" />
        <path class="monster-fuzz" d="m43 54-9 11 13 2-5 13m75-26 9 11-13 2 5 13" />
      </g>
      <g v-else-if="appearance.outfit === 'robot' && showsOutfitHeadwear" class="costume-head robot-helmet">
        <path class="robot-shell" d="M32 76V38q0-24 48-24t48 24v38l-14-8V43H46v25Z" />
        <path class="robot-panel" d="M49 26h62v17H49Z" /><circle class="robot-light" cx="61" cy="34" r="4" /><path class="robot-meter" d="M73 34h25" />
        <path class="robot-antenna" d="M80 14V4" /><circle class="robot-antenna-tip" cx="80" cy="3" r="6" />
        <path class="robot-ear" d="M29 52h17v26H29Zm85 0h17v26h-17Z" />
      </g>
      <g v-else-if="appearance.outfit === 'space' && showsOutfitHeadwear" class="costume-head astronaut-helmet">
        <circle class="helmet-glass" cx="80" cy="69" r="55" /><path class="helmet-rim" d="M38 105q42 15 84 0" />
        <path class="helmet-glint" d="M49 35q12-13 26-15" /><circle class="helmet-star" cx="119" cy="42" r="4" />
      </g>
      <g v-else-if="appearance.outfit === 'vampire'" class="costume-head vampire-face">
        <path class="vampire-brow" d="m48 58 22 3m42-3-22 3" />
        <path class="vampire-fang" d="M67 95h9l-4.5 10Zm17 0h9l-4.5 10Z" />
      </g>
      <g v-else-if="appearance.outfit === 'shark' && showsOutfitHeadwear" class="costume-head shark-hood">
        <path class="shark-hood-shell" d="M27 78C20 34 42 7 80 7s60 27 53 71l-15-15-8 18-12-14-18 13-18-13-12 14-8-18Z" />
        <path class="shark-top-fin" d="M66 13 79-4l17 19Z" />
        <path class="shark-mouth-rim" d="M39 66q41-43 82 0" />
        <path class="shark-teeth" d="m43 62 8 12 7-14 9 11 7-14 7 13 8-13 7 14 9-11 7 14 7-12" />
        <circle class="shark-eye" cx="48" cy="42" r="5" /><circle class="shark-eye" cx="112" cy="42" r="5" />
        <circle class="shark-eye-glint" cx="46" cy="40" r="1.6" /><circle class="shark-eye-glint" cx="110" cy="40" r="1.6" />
      </g>

      <component :is="accessoryVisual" v-if="accessoryVisual" />

      <g class="outfit-detail">
        <component :is="outfitVisual" v-if="outfitVisual" layer="front" />
        <template v-else-if="appearance.outfit === 'shirt'">
          <path class="adult-shirt" d="M45 138q8-12 23-16l12 13 12-13q15 4 23 16l3 42H42Z" />
          <path class="shirt-collar" d="m63 122 17 13-14 14-10-20m41-7-17 13 14 14 10-20M80 136v44" />
          <circle class="shirt-button" cx="80" cy="151" r="2.5" /><circle class="shirt-button" cx="80" cy="163" r="2.5" />
        </template>
        <template v-else-if="appearance.outfit === 'blouse'">
          <path class="adult-blouse" d="M44 139q9-13 25-17l11 12 11-12q16 4 25 17l2 41H42Z" />
          <path class="blouse-collar" d="m61 123 19 11-12 17-13-21m44-7-19 11 12 17 13-21" />
          <path class="blouse-detail" d="M80 135v45m-25-22q25 9 50 0" />
        </template>
        <template v-else-if="appearance.outfit === 'cardigan'">
          <path class="cardigan-shirt" d="M55 132q25-17 50 0l5 48H50Z" />
          <path class="cardigan" d="M44 138q8-12 24-16l12 14 12-14q16 4 24 16l2 42H91l-11-44-11 44H42Z" />
          <path class="cardigan-edge" d="m68 123 12 13 12-13M80 136v44" />
          <circle class="cardigan-button" cx="80" cy="151" r="2.5" /><circle class="cardigan-button" cx="80" cy="164" r="2.5" />
        </template>
        <template v-else-if="appearance.outfit === 'blazer'">
          <path class="blazer-shirt" d="M58 127h44l5 53H53Z" />
          <path class="blazer" d="M44 138q8-12 25-16l11 14 11-14q17 4 25 16l2 42H91l-11-44-11 44H42Z" />
          <path class="blazer-lapel" d="m67 123 13 13-15 20-9-27m37-6-13 13 15 20 9-27M80 136v44" />
          <circle class="blazer-button" cx="80" cy="158" r="3" />
        </template>
        <template v-else-if="appearance.outfit === 'hoodie'">
          <path class="hood-rim" d="M53 126c7 20 47 20 54 0-8-7-17-10-27-10s-19 3-27 10Z" />
          <path d="M75 134v16m10-16v16" /><circle class="hood-tip" cx="75" cy="152" r="3" /><circle class="hood-tip" cx="85" cy="152" r="3" />
          <path class="hood-pocket" d="M59 160q21-12 42 0l-5 16H64Z" />
        </template>
        <template v-else-if="appearance.outfit === 'overalls'">
          <path class="overall-shirt" d="M43 141Q48 127 61 123l19 12 19-12q13 4 18 18l-5 39H48Z" /><path class="overall-bib" d="M58 139q22-8 44 0l3 41H55Z" />
          <path d="m59 142-8-17m50 17 8-17M66 153q14-5 28 0v17H66Z" /><circle class="overall-button" cx="59" cy="142" r="4" /><circle class="overall-button" cx="101" cy="142" r="4" />
        </template>
        <template v-else-if="appearance.outfit === 'explorer'">
          <path class="explorer-vest" d="M49 130h24l7 14 7-14h24l4 50H45Z" />
          <path class="scarf" d="m59 122 21 22 21-22-8 34-13-12-13 12Z" /><path d="M49 151h20v18H49Zm42 0h20v18H91ZM80 145v35" />
        </template>
        <template v-else-if="appearance.outfit === 'space'">
          <path class="space-collar" d="M53 126q27 28 54 0l8 18q-35 20-70 0Z" />
          <rect class="space-panel" x="57" y="150" width="46" height="25" rx="7" /><circle class="space-light light-one" cx="67" cy="160" r="4" /><circle class="space-light light-two" cx="79" cy="160" r="4" /><path d="M90 159h8m-8 7h8" />
        </template>
        <template v-else-if="appearance.outfit === 'sporty'">
          <path class="sport-shirt" d="M43 142Q48 128 62 123l18 15 18-15q14 5 19 19l-5 38H48Z" /><path d="m58 125 22 17 22-17M50 143l-6 10m66-10 6 10" />
          <text class="sport-number" x="80" y="174">7</text>
        </template>
        <template v-else-if="appearance.outfit === 'pajamas'">
          <path class="pajama-top" d="M46 142q5-14 19-19l15 10 15-10q14 5 19 19l-4 38H50Z" /><path d="M80 126v54M50 146q30-8 60 0" />
          <path class="pajama-moon" d="M64 152c-9 3-9 17 2 19 6 1 10-3 12-7-10 3-15-4-14-12Z" />
          <path class="pajama-star" d="m96 153 3 6 7 1-5 5 1 7-6-3-6 3 1-7-5-5 7-1Z" />
        </template>
        <template v-else-if="appearance.outfit === 'superhero'">
          <path class="hero-collar" d="m51 126 29 20 29-20-10 30H61Z" />
          <path class="hero-shield" d="m80 144 17 7-3 18-14 10-14-10-3-18Z" /><text class="hero-letter" x="80" y="170">L</text>
          <path class="hero-belt" d="M39 174h82" /><circle class="hero-buckle" cx="80" cy="174" r="6" />
        </template>
        <template v-else-if="appearance.outfit === 'dinosaur'">
          <path class="dino-belly" d="M57 141q23-16 46 0l7 39H50Z" />
          <circle class="dino-dot" cx="69" cy="151" r="3" /><circle class="dino-dot" cx="92" cy="158" r="4" /><circle class="dino-dot" cx="72" cy="171" r="3" />
          <path class="dino-claws" d="m36 157 8-6 1 10 9-5m70 1-8-6-1 10-9-5" />
        </template>
        <template v-else-if="appearance.outfit === 'monster'">
          <path class="monster-belly" d="M53 138q27-18 54 0l8 42H45Z" /><path class="monster-belly-fuzz" d="m53 140 9 8 9-8 9 8 9-8 9 8 9-8" />
          <circle class="monster-spot spot-one" cx="67" cy="160" r="6" /><circle class="monster-spot spot-two" cx="94" cy="169" r="8" />
          <path class="monster-claws" d="m29 169 8-8 3 10 8-7m83 5-8-8-3 10-8-7" />
        </template>
        <template v-else-if="appearance.outfit === 'vampire'">
          <path class="vampire-shirt" d="M45 137q7-12 21-15l14 17 14-17q14 3 21 15l3 43H42Z" />
          <path class="vampire-collar" d="m50 122 30 18-19 14-12-25m61-7-30 18 19 14 12-25" />
          <path class="vampire-vest" d="m66 136 14 8 14-8 7 44H59Z" />
          <path class="vampire-chain" d="M68 151q12 13 24 0" /><circle class="vampire-gem" cx="80" cy="163" r="6" />
        </template>
        <template v-else-if="appearance.outfit === 'shark'">
          <path class="shark-body" d="M43 138q10-16 25-17l12 14 12-14q15 1 25 17l4 42H39Z" />
          <path class="shark-belly" d="M61 137q19-12 38 0l7 43H54Z" />
          <path class="shark-gills" d="m48 146 10 4m-11 4 10 4m55-12-10 4m11 4-10 4" />
          <path class="shark-splash" d="m68 166 5-5 7 5 7-5 6 5" />
        </template>
        <template v-else-if="appearance.outfit === 'robot'">
          <path class="robot-body" d="M43 134h74v46H43Z" /><rect class="robot-chest" x="55" y="142" width="50" height="28" rx="4" />
          <circle class="robot-button robot-red" cx="66" cy="152" r="4" /><circle class="robot-button robot-yellow" cx="78" cy="152" r="4" /><circle class="robot-button robot-green" cx="90" cy="152" r="4" />
          <path class="robot-grille" d="M64 163h32M80 170v10" />
        </template>
        <template v-else-if="appearance.outfit === 'fairy'">
          <path class="fairy-collar" d="m55 125 12 18 13-18 13 18 12-18-5 38H60Z" />
          <path class="fairy-leaf" d="M80 145c15 6 14 20 0 31-14-11-15-25 0-31Z" /><path d="M80 150v22" />
        </template>
        <template v-else>
          <path class="party-collar" d="m47 126 11 14 11-14 11 14 11-14 11 14 11-14 5 54H42Z" />
          <path class="party-star" d="m80 143 6 12 13 2-10 9 3 13-12-6-12 6 3-13-10-9 13-2Z" />
        </template>
      </g>

      <g class="seasonal">
        <component :is="seasonalAccessoryVisual" v-if="seasonalAccessoryVisual" />
        <g v-else-if="appearance.seasonalAccessoryId === 'witch'" class="witch">
          <path class="witch-top" d="M42 40 62 19 83 1l17 29 21 12Z" /><path class="witch-fold" d="M83 1q23 11 17 29" />
          <path class="witch-brim" d="M17 42q63-30 126 0-6 14-63 14T17 42Z" /><path class="witch-band" d="m54 31 56 2-4 15-59 1Z" /><path class="witch-buckle" d="M75 32h18v15H75Z" />
          <path class="witch-charm" d="m126 40 8 11 10-1-6 8 5 9-11-3-7 8-1-11-10-4 10-6Z" />
        </g>
        <g v-else-if="appearance.seasonalAccessoryId === 'pumpkin'" class="pumpkin">
          <path class="pumpkin-hood" d="M37 70C30 32 49 12 80 12s50 20 43 58l-9-17-8 12-11-17-15 14-15-14-11 17-8-12Z" />
          <path class="pumpkin-lines" d="M55 22q-11 17-9 31m59-31q11 17 9 31M80 13v28" /><path class="pumpkin-stem" d="M80 13q-3-12 8-13" /><path class="pumpkin-leaf" d="M84 6q13-8 16 2-10 5-16-2Z" />
        </g>
        <g v-else-if="appearance.seasonalAccessoryId === 'santa'" class="santa">
          <path class="santa-hat" d="M38 42C50 4 93-5 121 28L92 43Z" /><path class="santa-fold" d="M87 10q19 2 34 18" /><circle class="santa-pom" cx="121" cy="28" r="10" /><path class="santa-rim" d="M36 39q43-14 85 0l-3 12q-39-11-79 1Z" />
          <path class="santa-holly" d="M49 38q-9-10-13-1 7 7 13 1Zm0 0q9-10 13-1-7 7-13 1Z" /><circle class="santa-berry" cx="49" cy="39" r="4" />
        </g>
        <g v-else-if="appearance.seasonalAccessoryId === 'reindeer'" class="reindeer">
          <path class="antlers" d="M51 31 38 13m8 9-12-1m12 1 1-14m62 23 13-18m-8 9 12-1m-12 1-1-14" />
          <path class="reindeer-band" d="M43 39q37-23 74 0" /><path class="reindeer-ear" d="M48 31Q30 23 31 38q9 8 17-7Zm64 0q18-8 17 7-9 8-17-7Z" />
          <circle class="reindeer-nose" cx="80" cy="82" r="7" /><circle class="reindeer-shine" cx="78" cy="79" r="2" />
        </g>
        <g v-else-if="appearance.seasonalAccessoryId === 'bat'" class="bat-costume">
          <path class="bat-wing" d="M43 127C20 108 4 117 9 141l11-7 7 11 9-8 13 17Zm74 0c23-19 39-10 34 14l-11-7-7 11-9-8-13 17Z" />
          <path class="bat-band" d="M43 40q37-24 74 0" /><path class="bat-ear" d="m49 33 4-22 15 20m43 2-4-22-15 20" />
        </g>
        <g v-else-if="appearance.seasonalAccessoryId === 'elf'" class="elf">
          <path class="elf-hat" d="M37 43Q49 8 83 8l35 1-25 14 21 16Z" /><path class="elf-fold" d="M82 8q20 3 31 13" /><circle class="elf-bell" cx="118" cy="9" r="7" />
          <path class="elf-rim" d="M36 40q43-14 85 0l-3 11q-39-10-79 1Z" /><path class="elf-ear" d="m39 69-20-11 14 25m88-14 20-11-14 25" />
        </g>
        <g v-else-if="appearance.seasonalAccessoryId === 'snow-monster'" class="snow-monster">
          <path class="snow-hood" d="M30 82C18 44 36 9 80 9s62 35 50 73l-13-19-8 19-12-17-17 15-17-15-12 17-8-19Z" />
          <path class="snow-ear" d="M39 31 27 11 51 22m70 9 12-20-24 11" /><circle class="snow-pom" cx="31" cy="13" r="7" /><circle class="snow-pom" cx="129" cy="13" r="7" />
          <path class="snow-brow" d="M51 58q10-7 18 0m22 0q10-7 18 0" />
        </g>
      </g>
    </svg>
  </span>
</template>

<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';

import { avatarHairColors, avatarOutfitColors, avatarSkinToneColors } from '@/domain/avatar';
import { CHARACTER_REACTION_DURATION_MS } from '@/shared/runtime-timing';
import type { AvatarAppearance } from '@/domain/avatar';
import { avatarAccessoryVisuals, avatarExtensionVisualFor } from './avatar-visuals';

const props = withDefaults(defineProps<{ appearance: AvatarAppearance; size?: number; fullBody?: boolean; calm?: boolean }>(), { size: 64, fullBody: false, calm: false });
const { t } = useI18n();
const emit = defineEmits<{ interact: [] }>();
const isReacting = ref(false);
let reactionTimer: number | undefined;
const react = () => {
  emit('interact');
  isReacting.value = false;
  window.clearTimeout(reactionTimer);
  requestAnimationFrame(() => { isReacting.value = true; });
  reactionTimer = window.setTimeout(() => { isReacting.value = false; }, CHARACTER_REACTION_DURATION_MS);
};
const outfitHasHeadwear = computed(() => ['superhero', 'dinosaur', 'monster', 'shark', 'robot', 'space'].includes(props.appearance.outfit));
const showsOutfitHeadwear = computed(() => props.appearance.seasonalAccessoryId === 'none');
const allowsHeadAccessory = computed(() => (props.appearance.seasonalAccessoryId === 'none' || props.appearance.seasonalAccessoryId === 'bat') && !outfitHasHeadwear.value);
const accessoryVisual = computed(() => {
  if (props.appearance.accessoryId === 'none') {return undefined;}
  const definition = avatarAccessoryVisuals[props.appearance.accessoryId];
  return definition.requiresHeadClearance && !allowsHeadAccessory.value ? undefined : definition.component;
});
const faceVisual = computed(() => avatarExtensionVisualFor('face', props.appearance.face));
const faceShapeVisual = computed(() => avatarExtensionVisualFor('face-shape', props.appearance.faceShape));
const funAccessoryVisual = computed(() => props.appearance.funAccessoryId === 'none'
  ? undefined
  : avatarExtensionVisualFor('fun-accessory', props.appearance.funAccessoryId));
const hairVisual = computed(() => avatarExtensionVisualFor('hair', props.appearance.hair));
const outfitVisual = computed(() => avatarExtensionVisualFor('outfit', props.appearance.outfit));
const seasonalAccessoryVisual = computed(() => props.appearance.seasonalAccessoryId === 'none'
  ? undefined
  : avatarExtensionVisualFor('seasonal-accessory', props.appearance.seasonalAccessoryId));
const phase = computed(() => {
  const fingerprint = `${props.appearance.hair}-${props.appearance.face}-${props.appearance.outfit}`;
  return -(fingerprint.split('').reduce((sum, character) => sum + character.charCodeAt(0), 0) % 37) / 10;
});
const figureStyle = computed(() => ({
  '--avatar-size': `${props.size}px`,
  '--avatar-height': `${Math.round(props.size * 1.625)}px`,
  '--avatar-phase': `${phase.value}s`,
  '--skin': avatarSkinToneColors[props.appearance.skinToneId],
  '--hair': avatarHairColors[props.appearance.hairColorId],
  '--outfit': avatarOutfitColors[props.appearance.outfitColorId],
}));
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.avatar-figure {
  width: var(--avatar-size);
  height: var(--avatar-size);
  @apply d-inline-flex flex-shrink-0 overflow-hidden;
  border-radius: 38%;
  background: linear-gradient(
    145deg,
    var(--lad-palette-surface),
    var(--lad-palette-background)
  );
  box-shadow: inset 0 -5px 0
    color-mix(in srgb, var(--lad-palette-teal-700) 8%, transparent);
}
.avatar-figure.full-body {
  height: var(--avatar-height);
  @apply overflow-visible;
  border-radius: 0;
  background: transparent;
  box-shadow: none;
}
.avatar-figure {
  @apply cursor-pointer;
}
.avatar-figure.reacting svg {
  animation: avatar-tap 620ms var(--lad-easing-pop);
}
svg {
  @apply w-100 h-100 overflow-visible;
  transform-origin: center bottom;
  animation: avatar-idle 9s var(--avatar-phase) ease-in-out infinite;
}
.avatar-figure.calm svg,
.avatar-figure.calm .waving-arm,
.avatar-figure.calm .mouth,
.avatar-figure.calm .blush,
.avatar-figure.calm :deep(.propeller-blade) {
  animation: none;
}
.backdrop {
  fill: var(--lad-palette-background);
}
.backdrop-dot {
  fill: var(--lad-palette-white);
  opacity: 0.7;
}
.dot-two {
  fill: var(--lad-palette-yellow);
  opacity: 0.32;
}
.rainbow path {
  fill: none;
  stroke-linecap: round;
  stroke-width: 8;
}
.rainbow path:nth-child(1) {
  stroke: var(--lad-palette-coral);
}
.rainbow path:nth-child(2) {
  stroke: var(--lad-palette-yellow);
}
.rainbow path:nth-child(3) {
  stroke: var(--lad-palette-teal-400);
}
.torso {
  fill: var(--outfit);
  stroke: var(--lad-palette-muted-700);
  stroke-width: 4;
}
.full-body-lower .trousers {
  fill: color-mix(in srgb, var(--outfit) 62%, var(--lad-palette-muted-700));
  stroke: var(--lad-palette-muted-700);
  stroke-linejoin: round;
  stroke-width: 4;
}
.full-body-lower .leg {
  fill: none;
  stroke: var(--skin);
  stroke-linecap: round;
  stroke-width: 15;
}
.full-body-lower .shoe {
  fill: var(--lad-palette-background);
  stroke: var(--lad-palette-muted-700);
  stroke-linejoin: round;
  stroke-width: 4;
}
.arms path {
  fill: none;
  stroke: var(--outfit);
  stroke-linecap: round;
  stroke-width: 15;
}
.arms circle {
  fill: var(--skin);
  stroke: var(--lad-palette-orange-600);
  stroke-width: 2.5;
}
.waving-arm {
  transform-box: fill-box;
  transform-origin: 10% 5%;
  animation: avatar-wave 18s var(--avatar-phase) ease-in-out infinite;
}
.outfit-vampire .arms path {
  stroke: var(--lad-palette-text);
}
.outfit-shark .arms path {
  stroke: var(--lad-palette-blue-450);
}
.neck,
.ear,
.head {
  fill: var(--skin);
  stroke: var(--lad-palette-orange-600);
  stroke-width: 2.5;
}
.hair {
  fill: var(--hair);
  stroke: var(--lad-palette-muted-750);
  stroke-linejoin: round;
  stroke-width: 2.5;
}
.hair-highlight,
.long-curls .curl-highlight,
.front-hair .curl-fringe {
  fill: none;
  stroke: color-mix(in srgb, var(--lad-palette-white) 30%, transparent);
  stroke-linecap: round;
  stroke-width: 3;
}
.braid-tie {
  fill: none;
  stroke: var(--lad-palette-red-300);
  stroke-width: 6;
}
.shaved-line {
  fill: none;
  stroke: var(--hair);
  stroke-linecap: round;
  stroke-width: 2;
  opacity: 0.58;
}
.eye {
  fill: var(--lad-palette-text);
  transform-box: fill-box;
  transform-origin: center;
  animation: avatar-blink 11s var(--avatar-phase) ease-in-out infinite;
}
.face-details .eye:first-of-type {
  animation: avatar-wink 19s var(--avatar-phase) ease-in-out infinite;
}
.eye-glint {
  fill: var(--lad-palette-white);
}
.eye-line,
.dreamy-eyes,
.mouth,
.eyebrows,
.tiny-nose {
  fill: none;
  stroke: var(--lad-palette-orange-650);
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 3;
}
.eye-line {
  transform-box: fill-box;
  transform-origin: center;
  animation: wink-bounce 14s var(--avatar-phase) ease-in-out infinite;
}
.dreamy-eyes {
  stroke-width: 3.5;
}
.eyebrows {
  stroke-width: 2.5;
}
.confident-brows {
  stroke-width: 3.2;
}
.tiny-nose {
  opacity: 0.65;
  stroke-width: 2;
}
.blush {
  fill: var(--lad-palette-red-300);
  opacity: 0.28;
  animation: cheek-glow 16s var(--avatar-phase) ease-in-out infinite;
}
.mouth {
  transform-box: fill-box;
  transform-origin: center top;
  animation: changing-smile 16s var(--avatar-phase) ease-in-out infinite;
}
.mouth.surprised {
  fill: var(--lad-palette-red-600);
  animation: surprised-bounce 11s var(--avatar-phase) ease-in-out infinite;
}
.confident-mouth {
  transform: rotate(-4deg);
}
.silly-mouth path:first-child {
  fill: var(--lad-palette-orange-650);
  stroke: var(--lad-palette-orange-650);
  stroke-width: 2.5;
}
.silly-mouth path:last-child {
  fill: var(--lad-palette-red-300);
  stroke: var(--lad-palette-red-500);
  stroke-width: 2;
}
.freckles {
  fill: var(--lad-palette-orange-500);
}
.whiskers {
  fill: none;
  stroke: var(--lad-palette-orange-650);
  stroke-linecap: round;
  stroke-width: 2;
}
.mustache {
  fill: var(--lad-palette-muted-750);
}
.senior-details {
  fill: none;
  stroke: var(--lad-palette-orange-500);
  stroke-linecap: round;
  stroke-width: 1.5;
  opacity: 0.7;
}
.senior-details .smile-line {
  opacity: 0.7;
}
.lashes path {
  fill: none;
  stroke: var(--lad-palette-muted-750-2);
  stroke-linecap: round;
  stroke-width: 2.4;
}
.lashes circle {
  fill: var(--lad-palette-white);
  stroke: none;
}
.clown-nose circle {
  fill: var(--lad-palette-red-400);
  stroke: var(--lad-palette-red-600);
  stroke-width: 2;
}
.clown-nose ellipse {
  fill: var(--lad-palette-red-100);
}
.monster-horns path {
  fill: var(--lad-palette-teal-400);
  stroke: var(--lad-palette-muted-700);
  stroke-linejoin: round;
  stroke-width: 3;
}
.monster-horns circle {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-width: 1.5;
}
.pirate-patch > path {
  fill: none;
  stroke: var(--lad-palette-text);
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 4;
}
.pirate-patch > path:nth-child(2) {
  fill: var(--lad-palette-text);
  stroke-width: 2.5;
}
.pirate-patch .pirate-star {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-width: 1;
}
.superhero-mask > path:first-child {
  fill: var(--lad-palette-indigo-750);
  stroke: var(--lad-palette-text);
  stroke-linejoin: round;
  stroke-width: 2.5;
}
.superhero-mask ellipse {
  fill: var(--skin);
  stroke: var(--lad-palette-text);
  stroke-width: 1.5;
}
.hero-mask-flare {
  fill: none;
  stroke: var(--lad-palette-yellow);
  stroke-linecap: round;
  stroke-width: 4;
}
.dino-cap {
  fill: var(--lad-palette-teal-400);
  stroke: var(--lad-palette-teal-700);
  stroke-linejoin: round;
  stroke-width: 3;
}
.dino-spine {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-linejoin: round;
  stroke-width: 2.5;
}
.dino-tooth {
  fill: var(--lad-palette-amber-100);
  stroke: var(--lad-palette-orange-500);
  stroke-linejoin: round;
  stroke-width: 1.5;
}
.dino-hood-eye {
  fill: var(--lad-palette-white);
  stroke: var(--lad-palette-teal-700);
  stroke-width: 2;
}
.monster-cap {
  fill: var(--lad-palette-violet-400);
  stroke: var(--lad-palette-violet-650);
  stroke-linejoin: round;
  stroke-width: 3;
}
.monster-horn {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-linejoin: round;
  stroke-width: 2.5;
}
.monster-hood-eye {
  fill: var(--lad-palette-white);
  stroke: var(--lad-palette-violet-650);
  stroke-width: 2;
}
.monster-hood-pupil {
  fill: var(--lad-palette-muted-700);
}
.monster-fuzz {
  fill: none;
  stroke: var(--lad-palette-purple-200);
  stroke-linejoin: round;
  stroke-width: 5;
}
.vampire-brow {
  fill: none;
  stroke: var(--lad-palette-muted-750);
  stroke-linecap: round;
  stroke-width: 3.5;
}
.vampire-fang {
  fill: var(--lad-palette-surface);
  stroke: var(--lad-palette-orange-650);
  stroke-linejoin: round;
  stroke-width: 1.5;
}
.shark-hood-shell,
.shark-top-fin {
  fill: var(--lad-palette-blue-450);
  stroke: var(--lad-palette-muted-700);
  stroke-linejoin: round;
  stroke-width: 3;
}
.shark-mouth-rim {
  fill: none;
  stroke: var(--lad-palette-blue-250);
  stroke-linecap: round;
  stroke-width: 5;
}
.shark-teeth {
  fill: var(--lad-palette-surface);
  stroke: var(--lad-palette-muted);
  stroke-linejoin: round;
  stroke-width: 1.2;
}
.shark-eye {
  fill: var(--lad-palette-text);
  stroke: var(--lad-palette-text);
  stroke-width: 1.5;
}
.shark-eye-glint {
  fill: var(--lad-palette-white);
}
.robot-shell,
.robot-ear {
  fill: var(--lad-palette-muted-350);
  stroke: var(--lad-palette-muted-700);
  stroke-linejoin: round;
  stroke-width: 3;
}
.robot-panel {
  fill: var(--lad-palette-muted-700);
  stroke: var(--lad-palette-text);
}
.robot-light,
.robot-antenna-tip {
  fill: var(--lad-palette-red-400);
  stroke: var(--lad-palette-red-600);
  stroke-width: 1.5;
}
.robot-meter,
.robot-antenna {
  fill: none;
  stroke: var(--lad-palette-yellow);
  stroke-linecap: round;
  stroke-width: 3;
}
.helmet-glass {
  fill: color-mix(in srgb, var(--lad-palette-blue-150) 20%, transparent);
  stroke: var(--lad-palette-background);
  stroke-width: 7;
}
.helmet-rim {
  fill: none;
  stroke: var(--lad-palette-blue-550);
  stroke-width: 6;
}
.helmet-glint {
  fill: none;
  stroke: color-mix(in srgb, var(--lad-palette-white) 90%, transparent);
  stroke-linecap: round;
  stroke-width: 5;
}
.helmet-star {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-width: 1.5;
}
.outfit-back {
  stroke-linecap: round;
  stroke-linejoin: round;
}
.superhero-cape {
  fill: var(--lad-palette-red-400);
  stroke: var(--lad-palette-red-600);
  stroke-width: 3;
}
.fairy-wings {
  fill: color-mix(in srgb, var(--lad-palette-blue-250) 60%, transparent);
  stroke: var(--lad-palette-teal-550);
  stroke-width: 3;
}
.fairy-wings path:last-child {
  fill: none;
  stroke: color-mix(in srgb, var(--lad-palette-white) 80%, transparent);
  stroke-width: 2;
}
.dino-spikes {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-width: 3;
}
.monster-fur {
  fill: var(--lad-palette-violet-400);
  stroke: var(--lad-palette-violet-650);
  stroke-width: 3;
}
.vampire-cape {
  fill: var(--lad-palette-text);
  stroke: var(--lad-palette-violet-850);
  stroke-width: 3;
}
.vampire-cape-lining {
  fill: var(--lad-palette-red-500);
  stroke: var(--lad-palette-muted-750);
  stroke-width: 2;
}
.shark-fins {
  fill: var(--lad-palette-blue-550);
  stroke: var(--lad-palette-muted-700);
  stroke-width: 3;
}
.outfit-detail {
  fill: none;
  stroke: var(--lad-palette-background);
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 3;
}
.adult-shirt,
.adult-blouse {
  fill: color-mix(in srgb, var(--outfit) 72%, var(--lad-palette-white));
  stroke: var(--lad-palette-muted-700);
}
.shirt-collar,
.blouse-collar,
.blouse-detail {
  fill: color-mix(in srgb, var(--lad-palette-white) 65%, transparent);
  stroke: var(--lad-palette-muted-700);
}
.shirt-button,
.cardigan-button,
.blazer-button {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-width: 1;
}
.cardigan-shirt,
.blazer-shirt {
  fill: var(--lad-palette-background);
  stroke: var(--lad-palette-muted-700);
}
.cardigan,
.blazer {
  fill: var(--outfit);
  stroke: var(--lad-palette-muted-700);
}
.cardigan-edge,
.blazer-lapel {
  fill: color-mix(in srgb, var(--lad-palette-white) 10%, transparent);
  stroke: var(--lad-palette-background);
}
.hood-rim {
  fill: color-mix(in srgb, var(--lad-palette-white) 12%, transparent);
}
.hood-tip {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-width: 1.2;
}
.hood-pocket {
  fill: color-mix(in srgb, var(--lad-palette-text) 15%, transparent);
}
.overall-shirt {
  fill: var(--lad-palette-yellow);
  stroke: none;
}
.overall-bib {
  fill: var(--lad-palette-blue-strong);
  stroke: var(--lad-palette-indigo-750);
}
.overall-button {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-width: 1.2;
}
.explorer-vest {
  fill: var(--lad-palette-orange-600);
  stroke: var(--lad-palette-orange-750);
}
.scarf {
  fill: var(--lad-palette-amber-450);
  stroke: var(--lad-palette-amber-650);
}
.space-collar {
  fill: var(--lad-palette-background);
  stroke: var(--lad-palette-blue-550);
}
.space-panel {
  fill: var(--lad-palette-indigo-750);
  stroke: var(--lad-palette-text);
}
.space-light {
  stroke: none;
}
.light-one {
  fill: var(--lad-palette-teal-400);
}
.light-two {
  fill: var(--lad-palette-yellow);
}
.sport-shirt {
  fill: color-mix(in srgb, var(--lad-palette-white) 8%, transparent);
}
.sport-number,
.hero-letter {
  fill: var(--lad-palette-amber-100);
  stroke: none;
  text-anchor: middle;
  font-family: sans-serif;
  font-size: 28px;
  @apply font-weight-black;
}
.pajama-top {
  fill: color-mix(in srgb, var(--lad-palette-text) 12%, transparent);
}
.pajama-moon {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
}
.pajama-star {
  fill: var(--lad-palette-amber-150);
  stroke: var(--lad-palette-amber-650);
}
.hero-collar {
  fill: var(--lad-palette-amber-250);
  stroke: var(--lad-palette-amber-650);
}
.hero-shield {
  fill: var(--lad-palette-indigo-750);
  stroke: var(--lad-palette-indigo-750);
}
.hero-letter {
  font-size: 21px;
}
.hero-belt {
  stroke: var(--lad-palette-yellow);
  stroke-width: 7;
}
.hero-buckle {
  fill: var(--lad-palette-amber-150);
  stroke: var(--lad-palette-amber-650);
}
.dino-belly {
  fill: var(--lad-palette-green-250);
  stroke: var(--lad-palette-teal-700);
}
.dino-dot {
  fill: var(--lad-palette-amber-150);
  stroke: none;
}
.dino-claws {
  stroke: var(--lad-palette-amber-150);
  stroke-width: 4;
}
.monster-belly {
  fill: var(--lad-palette-purple-200);
  stroke: var(--lad-palette-violet-650);
}
.monster-belly-fuzz {
  fill: none;
  stroke: var(--lad-palette-amber-100);
}
.monster-spot {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
}
.spot-two {
  fill: var(--lad-palette-teal-400);
  stroke: var(--lad-palette-teal-700);
}
.monster-claws {
  fill: none;
  stroke: var(--lad-palette-amber-150);
  stroke-width: 4;
}
.vampire-shirt {
  fill: var(--lad-palette-violet-850);
  stroke: var(--lad-palette-violet-850);
}
.vampire-collar {
  fill: var(--lad-palette-amber-100);
  stroke: var(--lad-palette-orange-650);
}
.vampire-vest {
  fill: var(--lad-palette-red-600);
  stroke: var(--lad-palette-muted-750);
}
.vampire-chain {
  fill: none;
  stroke: var(--lad-palette-yellow);
}
.vampire-gem {
  fill: var(--lad-palette-red-400);
  stroke: var(--lad-palette-red-600);
}
.shark-body {
  fill: var(--lad-palette-blue-450);
  stroke: var(--lad-palette-muted-700);
}
.shark-belly {
  fill: var(--lad-palette-blue-150);
  stroke: var(--lad-palette-blue-450);
}
.shark-gills {
  fill: none;
  stroke: var(--lad-palette-muted-700);
}
.shark-splash {
  fill: none;
  stroke: var(--lad-palette-blue-250);
}
.robot-body {
  fill: var(--lad-palette-muted-350);
  stroke: var(--lad-palette-muted-700);
}
.robot-chest {
  fill: var(--lad-palette-muted-700);
  stroke: var(--lad-palette-text);
}
.robot-button {
  stroke: none;
}
.robot-red {
  fill: var(--lad-palette-red-400);
}
.robot-yellow {
  fill: var(--lad-palette-yellow);
}
.robot-green {
  fill: var(--lad-palette-teal-400);
  stroke: none;
}
.robot-grille {
  fill: none;
  stroke: var(--lad-palette-blue-150);
}
.fairy-collar {
  fill: var(--lad-palette-violet-400);
  stroke: var(--lad-palette-violet-650);
}
.fairy-leaf {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
}
.party-collar {
  fill: color-mix(in srgb, var(--lad-palette-white) 15%, transparent);
  stroke: var(--lad-palette-amber-150);
}
.party-star {
  fill: var(--lad-palette-amber-250);
  stroke: var(--lad-palette-amber-600);
}
.seasonal {
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 3;
}
.witch-top,
.witch-brim {
  fill: var(--lad-palette-violet-650);
  stroke: var(--lad-palette-indigo-750);
}
.witch-fold {
  fill: none;
  stroke: var(--lad-palette-violet-400);
}
.witch-band {
  fill: var(--lad-palette-red-300);
  stroke: var(--lad-palette-red-600);
}
.witch-buckle {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
}
.witch-charm {
  fill: var(--lad-palette-teal-400);
  stroke: var(--lad-palette-muted-700);
}
.pumpkin-hood {
  fill: var(--lad-palette-orange-400);
  stroke: var(--lad-palette-amber-700);
}
.pumpkin-lines {
  fill: none;
  stroke: var(--lad-palette-amber-600);
}
.pumpkin-stem {
  fill: none;
  stroke: var(--lad-palette-muted-600-2);
  stroke-width: 5;
}
.pumpkin-leaf {
  fill: var(--lad-palette-green-500);
  stroke: var(--lad-palette-green-700);
}
.santa-hat {
  fill: var(--lad-palette-red-400);
  stroke: var(--lad-palette-red-600);
}
.santa-fold {
  fill: none;
  stroke: var(--lad-palette-red-300);
}
.santa-pom,
.santa-rim {
  fill: var(--lad-palette-surface);
  stroke: var(--lad-palette-muted-400);
}
.santa-holly {
  fill: var(--lad-palette-mint-450);
  stroke: var(--lad-palette-teal-700);
}
.santa-berry {
  fill: var(--lad-palette-red-400);
  stroke: var(--lad-palette-red-600);
}
.antlers {
  fill: none;
  stroke: var(--lad-palette-orange-650);
  stroke-width: 6;
}
.reindeer-band {
  fill: none;
  stroke: var(--lad-palette-orange-600);
  stroke-width: 5;
}
.reindeer-ear {
  fill: var(--lad-palette-orange-500);
  stroke: var(--lad-palette-orange-650);
}
.reindeer-nose {
  fill: var(--lad-palette-red-400);
  stroke: var(--lad-palette-red-600);
}
.reindeer-shine {
  fill: var(--lad-palette-red-100);
  stroke: none;
}
.bat-wing,
.bat-ear {
  fill: var(--lad-palette-violet-650);
  stroke: var(--lad-palette-text);
}
.bat-band {
  fill: none;
  stroke: var(--lad-palette-text);
  stroke-width: 5;
}
.elf-hat {
  fill: var(--lad-palette-mint-450);
  stroke: var(--lad-palette-teal-700);
}
.elf-fold {
  fill: none;
  stroke: var(--lad-palette-green-250);
}
.elf-bell {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
}
.elf-rim {
  fill: var(--lad-palette-red-300);
  stroke: var(--lad-palette-red-600);
}
.elf-ear {
  fill: var(--skin);
  stroke: var(--lad-palette-orange-600);
}
.snow-hood,
.snow-ear,
.snow-pom {
  fill: var(--lad-palette-blue-150);
  stroke: var(--lad-palette-blue-550);
}
.snow-brow {
  fill: none;
  stroke: var(--lad-palette-blue-550);
  stroke-width: 5;
}
@keyframes avatar-idle {
  0%,
  100% {
    transform: translateY(1px) rotate(-0.35deg);
  }
  50% {
    transform: translateY(-2px) rotate(0.35deg);
  }
}
@keyframes avatar-tap {
  0% {
    transform: translateY(0) scale(1);
  }
  35% {
    transform: translateY(-10px) scale(1.06) rotate(-4deg);
  }
  65% {
    transform: translateY(1px) scale(0.98) rotate(3deg);
  }
  100% {
    transform: translateY(0) scale(1);
  }
}
@keyframes avatar-blink {
  0%,
  45%,
  49%,
  100% {
    transform: scaleY(1);
  }
  47%,
  48% {
    transform: scaleY(0.1);
  }
}
@keyframes avatar-wink {
  0%,
  70%,
  74%,
  100% {
    transform: scaleY(1);
  }
  71%,
  73% {
    transform: scaleY(0.08);
  }
}
@keyframes wink-bounce {
  0%,
  65%,
  100% {
    transform: translateY(0) rotate(0);
  }
  69%,
  75% {
    transform: translateY(1px) rotate(-5deg);
  }
}
@keyframes changing-smile {
  0%,
  58%,
  100% {
    transform: scale(1, 0.45) translateY(-1px);
  }
  67%,
  82% {
    transform: scale(1.08, 1.18) translateY(0);
  }
}
@keyframes surprised-bounce {
  0%,
  100% {
    transform: scale(0.88);
  }
  50% {
    transform: scale(1.08);
  }
}
@keyframes cheek-glow {
  0%,
  58%,
  100% {
    opacity: 0.14;
    transform: scale(0.8);
  }
  67%,
  82% {
    opacity: 0.38;
    transform: scale(1.12);
  }
}
@keyframes avatar-wave {
  0%,
  72%,
  100% {
    transform: rotate(0);
  }
  76% {
    transform: rotate(-12deg);
  }
  80% {
    transform: rotate(7deg);
  }
  84% {
    transform: rotate(-9deg);
  }
  88% {
    transform: rotate(0);
  }
}
@include reduced-motion {
  svg,
  .eye,
  .eye-line,
  .mouth,
  .blush,
  .waving-arm,
  :deep(.propeller-blade) {
    animation: none;
  }
}
</style>
