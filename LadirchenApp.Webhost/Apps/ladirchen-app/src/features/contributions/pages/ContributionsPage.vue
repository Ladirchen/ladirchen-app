<template>
  <div class="page page-padding contributions-page">
    <v-alert
      v-if="store.permissions.canManageContent || store.permissions.canManageChildTasks"
      class="guardian-context mb-5"
      color="info"
      density="compact"
      icon="i-mdi:shield-account-outline"
      variant="tonal"
    >
      {{ t("contributions.guardianContext") }}
    </v-alert>

    <ContributionFilterPanel
      v-if="store.viewerRole !== 'child'"
      v-model:kind="filter"
      v-model:person="personFilter"
      v-model:scope="scopeFilter"
      v-model:status="statusFilter"
      class="page-filter mb-6"
      :category-options="categoryFilterOptions"
      collapsible
      description-key="contributions.filter.pageDescription"
      :open-count="openContributionCount"
      :person-options="personFilterOptions"
      :show-scope="false"
      :show-status="false"
    />

    <div class="schedule-and-focus">
      <section class="focus-goals mb-6">
        <SectionHeader :description="t('contributions.focus.description')" :title="t('contributions.focus.title')">
          <template v-if="store.permissions.canManageContent || store.permissions.canManageChildTasks" #action>
            <v-btn
              color="primary"
              prepend-icon="i-mdi:target"
              rounded="lg"
              size="small"
              variant="tonal"
              @click="openFocusDialog"
            >
              {{ t("contributions.focus.add") }}
            </v-btn>
          </template>
        </SectionHeader>
        <div v-if="openFocusGoals.length" class="focus-grid">
          <BrandedCard
            v-for="focus in openFocusGoals"
            :id="`focus-${focus.id}`"
            :key="focus.id"
            class="focus-card pa-4"
            tabindex="-1"
            tone="contributions"
          >
            <div class="d-flex align-start ga-3">
              <span class="focus-target-badge" aria-hidden="true">
                <span class="focus-target-badge__ring"></span>
                <span class="focus-target-badge__dot"></span>
                <v-icon icon="i-mdi:arrow-top-right-bold-outline" size="19" />
              </span>
              <div class="flex-grow-1 min-w-0">
                <div class="d-flex align-center justify-space-between flex-wrap ga-2">
                  <strong>{{ focus.title }}</strong>
                  <div class="d-flex align-center flex-wrap ga-1">
                    <span class="focus-owner-badge">
                      <span v-if="memberAppearance(focus.assigneeId)" class="focus-owner-badge__avatar">
                        <AvatarFigure :appearance="memberAppearance(focus.assigneeId)!" calm :size="24" />
                      </span>
                      <v-avatar v-else color="info" size="24" variant="tonal">
                        {{ memberAvatar(focus.assigneeId) }}
                      </v-avatar>
                      {{ memberName(focus.assigneeId) }}
                    </span>
                    <v-chip class="focus-kind-badge" color="warning" size="x-small" variant="tonal">
                      <span class="focus-kind-badge__target" aria-hidden="true">🎯</span>
                      {{ focusPeriodLabel(focus.period) }}
                    </v-chip>
                  </div>
                </div>
                <p v-if="focusDescription(focus.description)" class="text-caption text-medium-emphasis">
                  {{ focusDescription(focus.description) }}
                </p>
                <div class="focus-deadline mt-2">
                  <span class="focus-deadline__icon" aria-hidden="true">⏳</span>
                  <span>
                    <strong>{{ dueCountdown(focus.endDate) }}</strong>
                    <small>{{ dueDeadline(focus.endDate) }}</small>
                  </span>
                </div>
              </div>
            </div>
            <v-progress-linear class="mt-4" color="success" height="10" rounded :model-value="focus.progressPercent" />
            <div class="d-flex justify-space-between text-caption mt-1">
              <span>{{ focus.progressPercent }} %</span>
              <span>{{ t("contributions.focus.remaining", { value: focus.remainingPercent }) }}</span>
            </div>
            <div class="d-flex align-center justify-space-between ga-2 mt-3">
              <v-chip v-if="focus.completionBonus > 0" color="warning" size="small" variant="tonal">
                5 ★ · +{{ focus.completionBonus }} L
              </v-chip>
              <span v-else></span>
              <div class="d-flex align-center ga-2">
                <v-btn
                  v-if="store.permissions.canManageContent && isOverdue(focus.endDate)"
                  color="error"
                  prepend-icon="i-mdi:archive-remove-outline"
                  rounded="lg"
                  size="small"
                  variant="tonal"
                  @click="requestOverdueResolution(focus)"
                >
                  {{ t("contributions.resolveOverdue.action") }}
                </v-btn>
                <v-btn
                  v-if="store.permissions.canManageContent || store.permissions.canManageChildTasks"
                  color="primary"
                  prepend-icon="i-mdi:calendar-plus"
                  rounded="lg"
                  size="small"
                  variant="tonal"
                  @click="openPreparationDialog(focus.id)"
                >
                  {{ t("contributions.focus.addPreparation") }}
                </v-btn>
              </div>
            </div>
          </BrandedCard>
        </div>
        <p v-else class="text-caption text-medium-emphasis">{{ t("contributions.focus.empty") }}</p>
        <div v-if="completedFocusGoals.length" class="mt-5">
          <strong class="text-subtitle-2">{{ t("contributions.focus.achievedTitle") }}</strong>
          <v-list class="focus-achieved-list mt-2" rounded="xl">
            <v-list-item
              v-for="focus in completedFocusGoals"
              :key="focus.id"
              :subtitle="memberName(focus.assigneeId)"
              :title="focus.title"
            >
              <template #prepend>
                <v-avatar color="success" size="36" variant="tonal">
                  <v-icon icon="i-mdi:check-bold" />
                </v-avatar>
              </template>
              <template #append>
                <div class="d-flex align-center ga-2">
                  <v-chip color="success" prepend-icon="i-mdi:trophy-outline" size="small" variant="tonal">
                    {{ t("contributions.focus.achieved") }}
                  </v-chip>
                  <v-btn
                    v-if="store.permissions.canManageContent"
                    :aria-label="t('contributions.resolveOverdue.focusAria', { title: focus.title })"
                    color="error"
                    icon="i-mdi:archive-remove-outline"
                    rounded="lg"
                    size="small"
                    variant="tonal"
                    @click="requestOverdueResolution(focus)"
                  />
                </div>
              </template>
            </v-list-item>
          </v-list>
        </div>
      </section>

      <v-card class="task-calendar mb-6" elevation="0" rounded="xl">
        <div class="calendar-toolbar d-flex align-center justify-space-between flex-wrap ga-3 mb-3">
          <div>
            <strong>{{ t("contributions.calendar.title") }}</strong>
            <p class="text-caption text-medium-emphasis">{{ calendarRangeLabel }}</p>
          </div>
          <div class="d-flex align-center ga-1">
            <v-btn
              :aria-expanded="calendarExpanded"
              :prepend-icon="calendarToggleIcon"
              color="primary"
              rounded="lg"
              size="small"
              variant="tonal"
              @click="calendarExpanded = !calendarExpanded"
            >
              {{ calendarToggleLabel }}
            </v-btn>
            <v-btn
              :aria-label="t('contributions.calendar.previous')"
              color="primary"
              icon="i-mdi:chevron-left"
              rounded="lg"
              size="small"
              variant="tonal"
              @click="weekOffset -= calendarWeekSpan"
            />
            <v-btn
              color="primary"
              prepend-icon="i-mdi:calendar-today-outline"
              rounded="lg"
              size="small"
              variant="tonal"
              @click="showCurrentWeek"
            >
              {{ t("contributions.calendar.today") }}
            </v-btn>
            <v-btn
              :aria-label="t('contributions.calendar.next')"
              color="primary"
              icon="i-mdi:chevron-right"
              rounded="lg"
              size="small"
              variant="tonal"
              @click="weekOffset += calendarWeekSpan"
            />
          </div>
        </div>
        <div v-if="calendarExpanded && calendarLegend.length" class="calendar-legend mb-3">
          <div v-for="member in calendarLegend" :key="member.memberId ?? 'open'" class="calendar-legend__member">
            <span v-if="member.appearance" class="calendar-member-avatar">
              <AvatarFigure :appearance="member.appearance" calm :size="34" />
            </span>
            <v-avatar v-else :color="member.color" size="32" variant="tonal">{{ member.avatar }}</v-avatar>
            <span>{{ member.title }}</span>
            <v-chip size="x-small" variant="tonal">{{ member.taskCount }}</v-chip>
          </div>
        </div>
        <div
          v-if="calendarExpanded && calendarLegend.length"
          class="week-grid"
          :class="{ 'week-grid--multiweek': calendarWeekSpan > 1 }"
        >
          <v-card
            v-for="day in calendarDays"
            :key="day.iso"
            class="week-day"
            :class="{ 'week-day--today': day.isToday }"
            elevation="0"
            rounded="lg"
          >
            <div class="week-day__heading">
              <span>{{ day.weekday }}</span>
              <strong>{{ day.day }}</strong>
            </div>
            <button
              v-for="entry in day.entries"
              :key="entry.contribution?.id ?? entry.focus?.id"
              class="calendar-task"
              :class="{
                'calendar-task--completed':
                  entry.contribution?.status === 'approved' || Boolean(entry.focus?.completed),
                'calendar-task--focus-goal': entry.focus && !entry.contribution,
                'calendar-task--focus-preparation': entry.focus && entry.contribution,
              }"
              :style="{ '--calendar-task-color': entry.color }"
              :title="entry.contribution?.title ?? entry.focus?.title"
              type="button"
              @click="openCalendarEntry(entry)"
            >
              <template v-if="entry.contribution">
                <SchabiTaskIcon v-if="isSchabiContributionId(entry.contribution.id)" />
                <span v-else>{{ entry.contribution.icon }}</span>
              </template>
              <v-icon v-else icon="i-mdi:target" size="17" />
              <span class="calendar-task__copy">
                <span class="calendar-task__title">{{ entry.contribution?.title ?? entry.focus?.title }}</span>
                <small v-if="entry.focusLabel">{{ entry.focusLabel }}</small>
              </span>
              <v-icon
                v-if="entry.contribution?.status === 'approved' || entry.focus?.completed"
                color="success"
                icon="i-mdi:check-circle"
                size="17"
              />
              <v-icon
                v-if="entry.contribution?.recurrence && entry.contribution.recurrence !== 'none'"
                icon="i-mdi:repeat"
                size="14"
              />
            </button>
            <span v-if="!day.entries.length" class="week-day__empty">—</span>
          </v-card>
        </div>
        <p v-else-if="calendarExpanded" class="text-caption text-medium-emphasis">
          {{ t("contributions.calendar.empty") }}
        </p>
      </v-card>
    </div>

    <template v-if="store.viewerRole === 'child'">
      <section v-if="guardianTasksToRate.length" class="mb-6">
        <SectionHeader
          :description="t('contributions.childReview.description')"
          :title="t('contributions.childReview.title')"
        >
          <template #action>
            <v-chip color="secondary" size="small">{{ guardianTasksToRate.length }}</v-chip>
          </template>
        </SectionHeader>
        <div class="review-card-grid">
          <BrandedCard
            v-for="{ contribution, completedByLabel } in childReviewCards"
            :key="contribution.id"
            class="review-card child-review-card pa-4"
            tone="contributions"
          >
            <div class="d-flex align-center ga-3">
              <v-avatar
                class="task-icon-avatar task-icon-avatar--compact"
                color="blue-lighten-5"
                rounded="lg"
                size="50"
              >
                {{ contribution.icon }}
              </v-avatar>
              <div class="flex-grow-1">
                <strong>{{ contribution.title }}</strong>
                <ContributionDescription :text="contribution.description" />
                <p class="text-caption text-medium-emphasis">{{ completedByLabel }}</p>
              </div>
            </div>
            <p class="text-caption text-medium-emphasis mt-3">{{ t("contributions.childReview.question") }}</p>
            <v-rating v-model="ratings[contribution.id]" active-color="warning" class="my-2" density="compact" hover />
            <div class="d-grid review-actions ga-2">
              <v-btn rounded="lg" variant="tonal" @click="store.returnContribution(contribution.id)">
                {{ t("contributions.childReview.again") }}
              </v-btn>
              <v-btn
                class="raised-button"
                color="primary"
                rounded="lg"
                variant="flat"
                @click="approveWithRating(contribution.id)"
              >
                {{ t("contributions.childReview.rate") }}
              </v-btn>
            </div>
          </BrandedCard>
        </div>
      </section>

      <ContributionFilterPanel
        v-model:kind="filter"
        v-model:person="personFilter"
        v-model:scope="scopeFilter"
        v-model:status="statusFilter"
        class="mb-4"
        :category-options="categoryFilterOptions"
        collapsible
        :open-count="openContributionCount"
        :person-options="personFilterOptions"
        :show-person="false"
        :show-status="false"
      />

      <TransitionGroup class="contribution-card-list contribution-card-grid" name="list" tag="div">
        <ChildContributionCard
          v-for="contribution in filteredContributions"
          :key="contribution.id"
          :active-child-id="store.activeChildId"
          :contribution="contribution"
          :current-time-milliseconds="store.currentTimeMilliseconds"
          :family-members="store.members"
          :family-time-zone="store.familyTimeZone"
          :promotion="promotionFor(contribution.id)"
          :reward="store.rewardForContribution(contribution.id)"
          :tip="contributionTip(contribution)"
          @claim="store.claimContribution($event.id)"
          @invite="openTeamInvite"
          @submit="store.submitContribution($event.id)"
        />
      </TransitionGroup>
      <BrandedCard
        v-if="!filteredContributions.length"
        class="empty-contributions pa-5 text-center"
        tone="contributions"
      >
        <AnimatedCompletionMark :size="84" />
        <strong>{{ emptyStateTitle }}</strong>
        <p class="text-caption text-medium-emphasis mt-1">{{ emptyStateDescription }}</p>
        <v-btn class="mt-3" color="primary" rounded="lg" variant="tonal" @click="toggleEmptyStateFilter">
          {{ emptyStateActionLabel }}
        </v-btn>
      </BrandedCard>
    </template>

    <template v-else>
      <div class="guardian-status-filter d-flex align-center justify-space-between ga-3 mb-5">
        <strong class="text-body-small">{{ t("contributions.filter.status") }}</strong>
        <v-btn-toggle
          v-model="statusFilter"
          color="primary"
          density="compact"
          mandatory
          rounded="lg"
          variant="outlined"
        >
          <v-btn prepend-icon="i-mdi:progress-clock" value="open">
            {{ t("contributions.filter.statuses.open") }}
          </v-btn>
          <v-btn prepend-icon="i-mdi:check-circle-outline" value="completed">
            {{ t("contributions.filter.statuses.completed") }}
          </v-btn>
        </v-btn-toggle>
      </div>

      <section v-if="showReviewableTasks" class="mb-6">
        <SectionHeader :description="t('contributions.review.description')" :title="t('contributions.review.title')">
          <template #action>
            <v-chip color="warning" size="small">{{ reviewableTasks.length }}</v-chip>
          </template>
        </SectionHeader>
        <div class="d-flex flex-column ga-3">
          <BrandedCard
            v-for="{ contribution, metaLabel } in reviewableTaskCards"
            :key="contribution.id"
            class="review-card pa-4"
            tone="contributions"
          >
            <div class="d-flex align-center ga-3">
              <v-avatar
                class="task-icon-avatar task-icon-avatar--compact"
                color="blue-lighten-5"
                rounded="lg"
                size="50"
              >
                {{ contribution.icon }}
              </v-avatar>
              <div class="flex-grow-1">
                <strong>{{ contribution.title }}</strong>
                <ContributionDescription :text="contribution.description" />
                <p class="text-caption text-medium-emphasis">{{ metaLabel }}</p>
              </div>
            </div>
            <div class="review-card__assessment">
              <div class="d-flex align-center justify-space-between ga-2 mt-3">
                <p class="text-caption text-medium-emphasis">{{ t("contributions.review.question") }}</p>
                <v-chip color="warning" size="x-small" variant="tonal">
                  5 ★ = +{{ store.perfectRatingBonusPercent }} %
                </v-chip>
              </div>
              <v-rating
                v-model="ratings[contribution.id]"
                active-color="warning"
                class="my-2"
                density="compact"
                hover
              />
              <div class="d-grid review-actions ga-2">
                <v-btn rounded="lg" variant="tonal" @click="store.returnContribution(contribution.id)">
                  {{ t("contributions.review.return") }}
                </v-btn>
                <v-btn
                  class="raised-button"
                  color="primary"
                  rounded="lg"
                  variant="flat"
                  @click="approveWithRating(contribution.id)"
                >
                  {{ t("contributions.review.confirm") }}
                </v-btn>
              </div>
            </div>
          </BrandedCard>
        </div>
      </section>

      <BrandedCard
        v-else-if="store.permissions.canManageContent || store.permissions.canManageChildTasks"
        class="empty-review pa-5 mb-6 text-center"
        tone="contributions"
      >
        <div class="empty-icon mb-2">🌤️</div>
        <strong>{{ t("contributions.review.emptyTitle") }}</strong>
        <p class="text-caption text-medium-emphasis mt-1">{{ t("contributions.review.emptyDescription") }}</p>
      </BrandedCard>

      <template v-if="store.permissions.canManageContent || store.permissions.canManageChildTasks">
        <template v-if="store.permissions.canManageContent">
          <SectionHeader
            :description="t('contributions.promotions.description')"
            :title="t('contributions.promotions.title')"
          >
            <template #action>
              <v-btn
                :aria-label="t('contributions.promotions.add')"
                color="warning"
                icon="i-mdi:lightning-bolt"
                size="small"
                variant="tonal"
                @click="promotionDialog = true"
              />
            </template>
          </SectionHeader>
          <div class="d-flex flex-column ga-2 mb-6">
            <BrandedCard
              v-for="promotion in activePromotions"
              :key="promotion.id"
              class="promotion-row pa-3"
              interactive
              tone="contributions"
              role="button"
              tabindex="0"
              @click="selectedPromotion = promotion"
              @keydown.enter="selectedPromotion = promotion"
            >
              <div class="d-flex align-center ga-3">
                <v-avatar color="warning" size="40" variant="tonal">⚡</v-avatar>
                <div class="flex-grow-1">
                  <strong class="text-body-small">{{ promotion.title }}</strong>
                  <p class="text-caption text-medium-emphasis">
                    {{ contributionTitle(promotion.contributionId) }} ·
                    {{ t("contributions.promotions.includingTeamwork", { value: promotion.teamworkBonus }) }}
                  </p>
                  <PromotionCountdown class="mt-1" :deadline="promotion.deadline" :time-zone="store.familyTimeZone" />
                </div>
                <v-chip color="warning" size="small">×{{ promotion.multiplier }}</v-chip>
                <v-btn
                  :aria-label="t('contributions.promotions.deleteAria', { title: promotion.title })"
                  color="error"
                  prepend-icon="i-mdi:delete-outline"
                  rounded="lg"
                  size="small"
                  variant="tonal"
                  @click.stop="promotionToDelete = promotion"
                >
                  {{ t("common.delete") }}
                </v-btn>
              </div>
            </BrandedCard>
          </div>
        </template>

        <SectionHeader :description="t('contributions.manage.description')" :title="t('contributions.manage.title')">
          <template #action>
            <v-btn color="primary" icon="i-mdi:plus" size="small" variant="tonal" @click="openContributionDialog" />
          </template>
        </SectionHeader>
        <div class="contribution-card-list d-flex flex-column ga-5">
          <section v-for="group in managedContributionGroups" :key="group.memberId ?? 'open'">
            <div class="member-task-heading d-flex align-center ga-2 mb-2">
              <v-avatar color="primary" size="30" variant="tonal">
                {{ store.members.find((member) => member.id === group.memberId)?.avatar ?? "✨" }}
              </v-avatar>
              <strong>{{ group.title }}</strong>
              <v-chip size="x-small" variant="tonal">{{ group.contributions.length }}</v-chip>
            </div>
            <div class="managed-contribution-grid">
              <ChildContributionCard
                v-for="contribution in group.contributions"
                :key="contribution.id"
                :active-child-id="store.activeChildId"
                :allow-invite="false"
                :contribution="contribution"
                :current-time-milliseconds="store.currentTimeMilliseconds"
                :family-members="store.members"
                :family-time-zone="store.familyTimeZone"
                :personalize-assignee="false"
                :promotion="promotionFor(contribution.id)"
                :reward="store.rewardForContribution(contribution.id)"
                :show-participant-actions="false"
                :tip="contributionTip(contribution)"
              >
                <template #actions>
                  <div class="management-actions">
                    <v-select
                      class="assignment-select"
                      density="compact"
                      hide-details
                      :disabled="contribution.status !== 'available'"
                      :items="assignmentOptions"
                      item-title="title"
                      item-value="value"
                      :model-value="contribution.assigneeId ?? ''"
                      variant="outlined"
                      @update:model-value="assignContribution(contribution.id, $event)"
                    />
                    <div class="management-action-buttons">
                      <v-btn
                        v-if="contribution.status === 'approved'"
                        :aria-label="t('contributions.manage.reactivateAria', { title: contribution.title })"
                        color="primary"
                        prepend-icon="i-mdi:restore"
                        rounded="lg"
                        size="small"
                        variant="tonal"
                        @click="reactivateContribution(contribution)"
                      >
                        {{ t("contributions.manage.reactivate") }}
                      </v-btn>
                      <v-btn
                        v-else
                        :aria-label="t('contributions.manage.copyAria', { title: contribution.title })"
                        prepend-icon="i-mdi:content-copy"
                        rounded="lg"
                        size="small"
                        variant="tonal"
                        @click="copyContribution(contribution)"
                      >
                        {{ t("contributions.manage.copy") }}
                      </v-btn>
                      <v-btn
                        v-if="canConvertToFocus(contribution)"
                        :aria-label="t('contributions.manage.convertToFocusAria', { title: contribution.title })"
                        color="warning"
                        prepend-icon="i-mdi:target"
                        rounded="lg"
                        size="small"
                        variant="tonal"
                        @click="openFocusConversion(contribution)"
                      >
                        {{ t("contributions.manage.convertToFocus") }}
                      </v-btn>
                      <v-btn
                        v-if="contribution.status === 'available'"
                        :aria-label="t('contributions.manage.editAria', { title: contribution.title })"
                        color="info"
                        prepend-icon="i-mdi:pencil-outline"
                        rounded="lg"
                        size="small"
                        variant="tonal"
                        @click="openContributionEditor(contribution)"
                      >
                        {{ t("contributions.manage.edit") }}
                      </v-btn>
                      <v-btn
                        v-if="contribution.status !== 'approved' && isOverdue(contribution.dueDate)"
                        :aria-label="t('contributions.resolveOverdue.taskAria', { title: contribution.title })"
                        color="error"
                        prepend-icon="i-mdi:archive-remove-outline"
                        rounded="lg"
                        size="small"
                        variant="tonal"
                        @click="requestOverdueResolution(contribution)"
                      >
                        {{ t("contributions.resolveOverdue.action") }}
                      </v-btn>
                      <v-btn
                        :aria-label="t('contributions.manage.deleteAria', { title: contribution.title })"
                        color="error"
                        prepend-icon="i-mdi:delete-outline"
                        rounded="lg"
                        size="small"
                        variant="tonal"
                        @click="requestContributionDelete(contribution)"
                      >
                        {{ t("common.delete") }}
                      </v-btn>
                    </div>
                  </div>
                </template>
              </ChildContributionCard>
            </div>
          </section>
        </div>
      </template>
    </template>

    <v-dialog
      :model-value="Boolean(selectedCalendarContribution)"
      max-width="480"
      @update:model-value="selectedCalendarContribution = undefined"
    >
      <v-card v-if="selectedCalendarContribution" class="pa-5" rounded="xl">
        <div class="d-flex align-start ga-3">
          <v-avatar color="primary" rounded="lg" size="52" variant="tonal">
            <SchabiTaskIcon v-if="isSchabiContributionId(selectedCalendarContribution.id)" />
            <template v-else>{{ selectedCalendarContribution.icon }}</template>
          </v-avatar>
          <div class="flex-grow-1 min-w-0">
            <v-card-title class="pa-0">{{ selectedCalendarContribution.title }}</v-card-title>
            <v-card-subtitle class="pa-0 mt-1">
              {{ memberName(selectedCalendarContribution.assigneeId) }} · {{ selectedCalendarDueLabel }}
            </v-card-subtitle>
            <p v-if="selectedCalendarDeadline" class="text-caption text-medium-emphasis mt-1">
              {{ selectedCalendarDeadline }}
            </p>
          </div>
        </div>
        <ContributionDescription v-if="selectedCalendarDescription" class="mt-4" :text="selectedCalendarDescription" />
        <div class="d-flex justify-end mt-5">
          <v-btn rounded="lg" variant="tonal" @click="selectedCalendarContribution = undefined">
            {{ t("common.close") }}
          </v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog v-model="addDialog" max-width="440">
      <v-card class="pa-5" rounded="xl">
        <v-card-title class="pa-0 mb-1">
          {{ contributionDialogTitle }}
        </v-card-title>
        <v-card-subtitle class="pa-0 mb-4">{{ t("contributions.form.prototypeHint") }}</v-card-subtitle>
        <v-text-field v-model="newContribution.title" :label="t('common.title')" variant="outlined" />
        <v-textarea
          v-model="newContribution.description"
          :label="t('contributions.form.shortDescription')"
          rows="2"
          variant="outlined"
        />
        <v-text-field
          v-model="newContribution.icon"
          :label="t('contributions.form.icon')"
          maxlength="4"
          variant="outlined"
        />
        <v-combobox
          v-model="newContribution.category"
          :items="categorySuggestions"
          :label="t('contributions.form.category')"
          maxlength="80"
          variant="outlined"
        />
        <v-select
          v-if="contributionToEdit || newContribution.kind !== 'basic' || !store.permissions.canManageContent"
          v-model="newContribution.assigneeId"
          :items="assignmentOptions"
          item-title="title"
          item-value="value"
          :label="t('contributions.form.assignment')"
          variant="outlined"
        />
        <v-select
          v-else
          v-model="newContribution.assigneeIds"
          chips
          clearable
          :items="childAssignmentOptions"
          item-title="title"
          item-value="value"
          :label="t('contributions.form.children')"
          multiple
          variant="outlined"
        />
        <v-text-field
          v-if="!contributionToEdit"
          v-model.number="newContribution.quantity"
          :label="t('contributions.form.quantity')"
          max="20"
          min="1"
          type="number"
          variant="outlined"
        />
        <div class="d-grid form-columns ga-3">
          <v-text-field
            v-model="newContribution.dueDate"
            clearable
            :label="t('contributions.form.dueDate')"
            type="date"
            variant="outlined"
          />
          <v-select
            v-model="newContribution.recurrence"
            :items="recurrenceOptions"
            item-title="title"
            item-value="value"
            :label="t('contributions.form.recurrence')"
            variant="outlined"
          />
        </div>
        <div class="d-grid form-columns ga-3">
          <v-select
            v-model="newContribution.kind"
            :items="kindOptions"
            item-title="title"
            item-value="value"
            :label="t('contributions.form.kind')"
            variant="outlined"
          />
          <v-text-field
            v-model.number="newContribution.reward"
            :label="t('contributions.form.coins')"
            min="0"
            type="number"
            variant="outlined"
          />
        </div>
        <v-slider
          v-if="newContribution.kind !== 'extra'"
          v-model="newContribution.energy"
          color="primary"
          :label="t('contributions.form.energyShare')"
          max="50"
          min="5"
          step="5"
          thumb-label
        />
        <v-switch
          v-if="store.permissions.canManageContent"
          v-model="newContribution.isVisibleToChildren"
          color="primary"
          :hint="t('contributions.form.visibilityHint')"
          :label="t('contributions.form.visibleToChildren')"
          persistent-hint
        />
        <div class="d-flex justify-end ga-2 mt-2">
          <v-btn rounded="lg" variant="text" @click="closeContributionDialog">{{ t("common.cancel") }}</v-btn>
          <v-btn color="primary" :disabled="!canSaveContribution" rounded="lg" variant="flat" @click="saveContribution">
            {{ contributionSubmitLabel }}
          </v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog v-model="focusDialog" max-width="500">
      <v-card class="pa-5" rounded="xl">
        <v-card-title class="pa-0 mb-1">{{ focusDialogTitle }}</v-card-title>
        <v-card-subtitle class="pa-0 mb-4">{{ focusDialogDescription }}</v-card-subtitle>
        <v-text-field v-model="newFocus.title" :label="t('common.title')" variant="outlined" />
        <v-textarea
          v-model="newFocus.description"
          :label="t('contributions.form.shortDescription')"
          rows="2"
          variant="outlined"
        />
        <div class="d-grid form-columns ga-3">
          <v-text-field
            v-model="newFocus.icon"
            :label="t('contributions.form.icon')"
            maxlength="4"
            variant="outlined"
          />
          <v-select
            v-model="newFocus.period"
            :items="focusPeriodOptions"
            item-title="title"
            item-value="value"
            :label="t('contributions.focus.period')"
            variant="outlined"
          />
        </div>
        <v-select
          v-model="newFocus.assigneeId"
          :items="childAssignmentOptions"
          item-title="title"
          item-value="value"
          :label="t('contributions.form.assignment')"
          variant="outlined"
        />
        <div class="d-grid form-columns ga-3">
          <v-text-field
            v-model="newFocus.startDate"
            :label="t('contributions.focus.startDate')"
            type="date"
            variant="outlined"
          />
          <v-text-field
            v-model="newFocus.endDate"
            :label="t('contributions.focus.endDate')"
            type="date"
            variant="outlined"
          />
        </div>
        <v-text-field
          v-model.number="newFocus.completionBonus"
          :label="t('contributions.focus.completionBonus')"
          min="0"
          type="number"
          variant="outlined"
        />
        <div class="d-flex justify-end ga-2">
          <v-btn rounded="lg" variant="text" @click="closeFocusDialog">{{ t("common.cancel") }}</v-btn>
          <v-btn color="primary" :disabled="!canSaveFocus" rounded="lg" variant="flat" @click="saveFocus">
            {{ focusSubmitLabel }}
          </v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog v-model="preparationDialog" max-width="500">
      <v-card class="pa-5" rounded="xl">
        <v-card-title class="pa-0 mb-1">{{ t("contributions.focus.preparationTitle") }}</v-card-title>
        <v-card-subtitle class="pa-0 mb-4">{{ selectedFocus?.title }}</v-card-subtitle>
        <div v-for="(_, index) in preparationDates" :key="index" class="d-flex align-center ga-2">
          <v-text-field
            v-model="preparationDates[index]"
            :label="t('contributions.focus.preparationDay')"
            type="date"
            variant="outlined"
          />
          <v-btn
            v-if="preparationDates.length > 1"
            :aria-label="t('common.delete')"
            icon="i-mdi:close"
            size="small"
            variant="text"
            @click="preparationDates.splice(index, 1)"
          />
        </div>
        <v-btn class="mb-4" prepend-icon="i-mdi:plus" size="small" variant="text" @click="preparationDates.push('')">
          {{ t("contributions.focus.addDay") }}
        </v-btn>
        <div class="d-grid form-columns ga-3">
          <v-text-field
            v-model.number="preparationMinutes"
            :label="t('contributions.focus.minutes')"
            min="1"
            type="number"
            variant="outlined"
          />
          <v-text-field
            v-model.number="preparationReward"
            :label="t('contributions.focus.reward')"
            min="0"
            type="number"
            variant="outlined"
          />
        </div>
        <div class="d-flex justify-end ga-2">
          <v-btn rounded="lg" variant="text" @click="preparationDialog = false">{{ t("common.cancel") }}</v-btn>
          <v-btn color="primary" :disabled="!canSavePreparations" rounded="lg" variant="flat" @click="savePreparations">
            {{ t("common.add") }}
          </v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog
      :model-value="Boolean(contributionToDelete)"
      max-width="390"
      @update:model-value="contributionToDelete = undefined"
    >
      <v-card v-if="contributionToDelete" class="pa-5" rounded="xl">
        <v-avatar class="mb-3" color="error" size="52" variant="tonal">
          <v-icon icon="i-mdi:delete-alert-outline" />
        </v-avatar>
        <v-card-title class="pa-0">{{ t("contributions.deleteContribution.title") }}</v-card-title>
        <v-card-subtitle class="pa-0 mt-1 mb-4">{{ contributionToDelete.title }}</v-card-subtitle>
        <v-alert class="mb-4" color="warning" density="compact" variant="tonal">
          {{ contributionDeleteWarning }}
        </v-alert>
        <div class="d-flex justify-end ga-2">
          <v-btn rounded="lg" variant="text" @click="contributionToDelete = undefined">{{ t("common.cancel") }}</v-btn>
          <v-btn color="error" rounded="lg" variant="flat" @click="confirmContributionDelete">
            {{ t("common.delete") }}
          </v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog
      :model-value="Boolean(promotionToDelete)"
      max-width="390"
      @update:model-value="promotionToDelete = undefined"
    >
      <v-card v-if="promotionToDelete" class="pa-5" rounded="xl">
        <v-avatar class="mb-3" color="error" size="52" variant="tonal">
          <v-icon icon="i-mdi:calendar-remove-outline" />
        </v-avatar>
        <v-card-title class="pa-0">{{ t("contributions.deletePromotion.title") }}</v-card-title>
        <v-card-subtitle class="pa-0 mt-1 mb-4">{{ promotionToDelete.title }}</v-card-subtitle>
        <div class="d-flex justify-end ga-2">
          <v-btn rounded="lg" variant="text" @click="promotionToDelete = undefined">{{ t("common.cancel") }}</v-btn>
          <v-btn color="error" rounded="lg" variant="flat" @click="confirmPromotionDelete">
            {{ t("common.delete") }}
          </v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog
      :model-value="Boolean(overdueResolutionTarget)"
      max-width="410"
      @update:model-value="overdueResolutionTarget = undefined"
    >
      <v-card v-if="overdueResolutionTarget" class="pa-5" rounded="xl">
        <v-avatar class="mb-3" color="error" size="52" variant="tonal">
          <v-icon icon="i-mdi:archive-remove-outline" />
        </v-avatar>
        <v-card-title class="pa-0">{{ t("contributions.resolveOverdue.title") }}</v-card-title>
        <v-card-subtitle class="pa-0 mt-1 mb-4">{{ overdueResolutionTarget.title }}</v-card-subtitle>
        <v-alert class="mb-4" color="warning" density="compact" variant="tonal">
          {{ overdueResolutionWarning }}
        </v-alert>
        <div class="d-flex justify-end ga-2">
          <v-btn rounded="lg" variant="text" @click="overdueResolutionTarget = undefined">
            {{ t("common.cancel") }}
          </v-btn>
          <v-btn color="error" rounded="lg" variant="flat" @click="confirmOverdueResolution">
            {{ t("contributions.resolveOverdue.confirm") }}
          </v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog v-model="teamDialog" max-width="420">
      <v-card class="pa-5" rounded="xl">
        <div class="team-dialog-icon mb-3">🤝</div>
        <v-card-title class="pa-0 mb-1">{{ t("contributions.team.title") }}</v-card-title>
        <v-card-subtitle class="pa-0 mb-4">
          {{ t("contributions.team.description", { title: teamContribution?.title }) }}
        </v-card-subtitle>
        <v-select
          v-model="selectedPartnerIds"
          chips
          closable-chips
          :items="partnerOptions"
          item-title="title"
          item-value="value"
          :label="t('contributions.team.selectPartners')"
          multiple
          variant="outlined"
        />
        <div class="partner-rule mb-4">
          <v-icon color="info" size="18">i-mdi:shield-check-outline</v-icon>
          <span>{{ t("contributions.team.rule") }}</span>
        </div>
        <div class="d-flex justify-end ga-2">
          <v-btn rounded="lg" variant="text" @click="teamDialog = false">{{ t("common.cancel") }}</v-btn>
          <v-btn color="info" rounded="lg" variant="flat" @click="saveTeamInvite">
            {{ t("contributions.team.save") }}
          </v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog
      :model-value="Boolean(selectedPromotion)"
      max-width="410"
      @update:model-value="selectedPromotion = undefined"
    >
      <v-card v-if="selectedPromotion" class="pa-5" rounded="xl">
        <div class="promotion-detail-icon">⚡</div>
        <p class="eyebrow mt-3 mb-1">{{ t("contributions.promotions.singular") }}</p>
        <h2 class="promotion-detail-title">{{ selectedPromotion.title }}</h2>
        <p class="text-body-small text-medium-emphasis mt-2">
          {{
            t("contributions.promotions.deadline", {
              title: contributionTitle(selectedPromotion.contributionId),
              deadline: selectedPromotion.deadline,
            })
          }}
        </p>
        <PromotionCountdown class="mt-3" :deadline="selectedPromotion.deadline" :time-zone="store.familyTimeZone" />
        <div class="promotion-reward mt-4">
          <span>{{ t("contributions.promotions.reachableReward") }}</span>
          <strong>
            {{
              t("contributions.reward.coins", { value: store.rewardForContribution(selectedPromotion.contributionId) })
            }}
          </strong>
          <small>
            {{ t("contributions.promotions.includingTeamwork", { value: selectedPromotion.teamworkBonus }) }}
          </small>
        </div>
        <v-btn
          class="mt-5"
          color="warning"
          rounded="lg"
          variant="flat"
          width="100%"
          @click="selectedPromotion = undefined"
        >
          {{ t("common.understood") }}
        </v-btn>
      </v-card>
    </v-dialog>

    <v-dialog v-model="promotionDialog" max-width="440">
      <v-card class="pa-5" rounded="xl">
        <v-card-title class="pa-0 mb-1">{{ t("contributions.promotions.startTitle") }}</v-card-title>
        <v-card-subtitle class="pa-0 mb-4">{{ t("contributions.promotions.startDescription") }}</v-card-subtitle>
        <v-select
          v-model="newPromotion.contributionId"
          :items="promotionContributionOptions"
          item-title="title"
          item-value="value"
          :label="t('contributions.promotions.contribution')"
          variant="outlined"
        />
        <div class="d-grid form-columns ga-3">
          <v-select
            v-model="newPromotion.multiplier"
            :items="multiplierOptions"
            item-title="title"
            item-value="value"
            :label="t('contributions.promotions.bonus')"
            variant="outlined"
          />
          <v-text-field
            v-model="newPromotion.deadline"
            :label="t('contributions.promotions.validUntil')"
            type="time"
            variant="outlined"
          />
        </div>
        <div class="d-flex justify-end ga-2 mt-2">
          <v-btn rounded="lg" variant="text" @click="promotionDialog = false">{{ t("common.cancel") }}</v-btn>
          <v-btn color="warning" :disabled="!canStartPromotion" rounded="lg" variant="flat" @click="addPromotion">
            {{ t("contributions.promotions.start") }}
          </v-btn>
        </div>
      </v-card>
    </v-dialog>
  </div>
</template>

<script lang="ts" setup>
import { computed, nextTick, ref } from "vue";
import { useI18n } from "vue-i18n";
import { resolveFamilyMemberAvatarAppearance } from "@/domain/avatar";
import type { AvatarAppearance } from "@/domain/avatar";
import { dueDateParts } from "@/domain/contributions/due-date";
import { calendarDateInTimeZone } from "@/domain/shared/zoned-calendar";
import type { Contribution, FocusGoal, FocusPeriod, NewFocusGoal } from "@/domain/contributions/types";
import type { ContributionId, FamilyMemberId } from "@/domain/shared/identifiers";
import { isSchabiContributionId } from "@/domain/contributions/source";
import AnimatedCompletionMark from "@/shared/components/AnimatedCompletionMark.vue";
import AvatarFigure from "@/shared/components/avatar/AvatarFigure.vue";
import ChildContributionCard from "@/shared/components/contributions/ChildContributionCard.vue";
import ContributionDescription from "@/shared/components/contributions/ContributionDescription.vue";
import SchabiTaskIcon from "@/shared/components/contributions/SchabiTaskIcon.vue";
import ContributionFilterPanel from "@/features/contributions/components/ContributionFilterPanel.vue";
import PromotionCountdown from "@/shared/components/contributions/PromotionCountdown.vue";
import SectionHeader from "@/shared/components/ui/SectionHeader.vue";
import BrandedCard from "@/shared/components/ui/BrandedCard.vue";
import { useContributionsPage } from "@/features/contributions/composables/use-contributions-page";
import { familyMemberColorPalette } from "@/theme/color-palette";

interface CalendarEntry {
  color: string;
  contribution?: Contribution;
  focus?: FocusGoal;
  focusLabel?: string;
}

const {
  activePromotions,
  addDialog,
  addPromotion,
  approveWithRating,
  assignContribution,
  assignmentOptions,
  categoryFilterOptions,
  categorySuggestions,
  childAssignmentOptions,
  closeContributionDialog,
  copyContribution,
  reactivateContribution,
  reviewableTasks,
  confirmContributionDelete,
  confirmPromotionDelete,
  contributionTip,
  contributionTitle,
  contributionToEdit,
  contributionToDelete,
  filter,
  filteredContributions,
  guardianTasksToRate,
  kindOptions,
  localizedContributions,
  managedContributionGroups,
  memberName,
  multiplierOptions,
  newContribution,
  newPromotion,
  openContributionCount,
  openContributionDialog,
  openContributionEditor,
  openTeamInvite,
  personFilter,
  personFilterOptions,
  promotionContributionOptions,
  promotionDialog,
  promotionFor,
  promotionToDelete,
  ratings,
  requestContributionDelete,
  saveContribution,
  saveTeamInvite,
  scopeFilter,
  selectedPromotion,
  selectedPartnerIds,
  partnerOptions,
  statusFilter,
  store,
  teamContribution,
  teamDialog,
} = useContributionsPage();
const { locale, t } = useI18n();
const focusDialog = ref(false);
const preparationDialog = ref(false);
const selectedFocusId = ref<ContributionId>();
const overdueResolutionTarget = ref<{ id: ContributionId; title: string; isFocus: boolean }>();
const preparationDates = ref([""]);
const preparationMinutes = ref(30);
const preparationReward = ref(1);
const newFocus = ref<NewFocusGoal>(createEmptyFocus());
const contributionToConvert = ref<Contribution>();
const calendarExpanded = ref(true);
const selectedCalendarContribution = ref<Contribution>();
const selectedCalendarDueLabel = computed(() => {
  const contribution = selectedCalendarContribution.value;

  return contribution?.dueDate ? dueCountdown(contribution.dueDate) : (contribution?.dueLabel ?? "");
});
const selectedCalendarDeadline = computed(() => {
  const dueDate = selectedCalendarContribution.value?.dueDate;

  return dueDate ? dueDeadline(dueDate) : "";
});
const selectedCalendarDescription = computed(() => {
  const contribution = selectedCalendarContribution.value;

  if (!contribution) {
    return "";
  }

  return isSchabiContributionId(contribution.id)
    ? contribution.description.replace(/^Fällig am \d{2}\.\d{2}\.\d{4}(?:\s*\n\s*)?/, "").trim()
    : contribution.description;
});
const weekOffset = ref(0);
const calendarWeekSpan = computed(() => (store.viewerRole === "child" ? 1 : 3));
const calendarToggleIcon = computed(() => (calendarExpanded.value ? "i-mdi:eye-off-outline" : "i-mdi:eye-outline"));
const calendarToggleLabel = computed(() =>
  t(calendarExpanded.value ? "contributions.calendar.hide" : "contributions.calendar.show"),
);
const visibleFocusGoals = computed(() =>
  store.focusGoals.filter((focus) => {
    const matchesViewer = store.viewerRole !== "child" || focus.assigneeId === store.signedInMemberId;
    const matchesPerson = personFilter.value === "all" || focus.assigneeId === personFilter.value;
    const matchesCategory =
      filter.value === "all" ||
      localizedContributions.value.some(
        (contribution) =>
          focus.preparationContributionIds.includes(contribution.id) && contribution.category === filter.value,
      );

    return matchesViewer && matchesPerson && matchesCategory;
  }),
);
const openFocusGoals = computed(() => visibleFocusGoals.value.filter((focus) => !focus.completed));
const completedFocusGoals = computed(() => visibleFocusGoals.value.filter((focus) => focus.completed));
const selectedFocus = computed(() => store.focusGoals.find((focus) => focus.id === selectedFocusId.value));
const today = computed(() => calendarDateInTimeZone(new Date(store.currentTimeMilliseconds), store.familyTimeZone));
const overdueResolutionWarning = computed(() =>
  t(
    overdueResolutionTarget.value?.isFocus
      ? "contributions.resolveOverdue.focusWarning"
      : "contributions.resolveOverdue.taskWarning",
  ),
);
const focusPeriodOptions = computed(() => [
  { title: t("contributions.focus.periodDate"), value: "date" },
  { title: t("contributions.focus.periodWeekly"), value: "weekly" },
  { title: t("contributions.focus.periodMonthly"), value: "monthly" },
]);
const canSaveFocus = computed(
  () =>
    newFocus.value.title.trim().length > 0 &&
    newFocus.value.assigneeId.length > 0 &&
    newFocus.value.startDate.length > 0 &&
    newFocus.value.endDate >= newFocus.value.startDate &&
    newFocus.value.completionBonus >= 0,
);
const focusDialogTitle = computed(() =>
  t(contributionToConvert.value ? "contributions.focus.convertTitle" : "contributions.focus.formTitle"),
);
const focusSubmitLabel = computed(() => t(contributionToConvert.value ? "common.save" : "common.add"));
const focusDialogDescription = computed(() =>
  t(contributionToConvert.value ? "contributions.focus.convertDescription" : "contributions.focus.formDescription"),
);
const canSavePreparations = computed(
  () =>
    Boolean(selectedFocus.value) &&
    preparationDates.value.length > 0 &&
    preparationDates.value.every(
      (date) => date >= (selectedFocus.value?.startDate ?? "") && date <= (selectedFocus.value?.endDate ?? ""),
    ) &&
    preparationMinutes.value > 0 &&
    preparationReward.value >= 0,
);
function createEmptyFocus(): NewFocusGoal {
  const today = localIsoDate(new Date());

  return {
    assigneeId: "" as FamilyMemberId,
    completionBonus: 5,
    description: "",
    endDate: today,
    icon: "🎯",
    period: "date",
    startDate: today,
    title: "",
  };
}
function focusPeriodLabel(period: FocusPeriod): string {
  if (period === "weekly") {
    return t("contributions.focus.periodWeekly");
  }

  if (period === "monthly") {
    return t("contributions.focus.periodMonthly");
  }

  return t("contributions.focus.periodDate");
}
function deadlineParts(endDate: string) {
  return dueDateParts(endDate, store.currentTimeMilliseconds, store.familyTimeZone, locale.value);
}
function dueCountdown(endDate: string): string {
  const parts = deadlineParts(endDate);

  if (!parts || parts.daysRemaining === 0) {
    return t("contributions.due.today");
  }

  return t(parts.daysRemaining === 1 ? "contributions.due.oneDay" : "contributions.due.days", {
    days: parts.daysRemaining,
  });
}
function dueDeadline(endDate: string): string {
  const parts = deadlineParts(endDate);

  return parts ? t("contributions.due.deadline", { date: parts.date, weekday: parts.weekday }) : "";
}
function focusDescription(description: string): string {
  return description.replace(/^Schabi-Termin am \d{2}\.\d{2}\.\d{4}$/, "").trim();
}
function memberAppearance(memberId: FamilyMemberId): AvatarAppearance | undefined {
  const member = store.members.find((item) => item.id === memberId);

  return member ? resolveFamilyMemberAvatarAppearance(member, store.members) : undefined;
}
function memberAvatar(memberId: FamilyMemberId): string {
  return store.members.find((member) => member.id === memberId)?.avatar ?? "✨";
}
async function saveFocus() {
  if (!canSaveFocus.value) {
    return;
  }

  if (contributionToConvert.value) {
    await store.convertContributionToFocus(contributionToConvert.value.id, { ...newFocus.value });
  } else {
    await store.addFocus({ ...newFocus.value });
  }

  closeFocusDialog();
}
function openFocusDialog() {
  contributionToConvert.value = undefined;
  newFocus.value = createEmptyFocus();
  focusDialog.value = true;
}
function openFocusConversion(contribution: Contribution) {
  if (!canConvertToFocus(contribution) || !contribution.assigneeId) {
    return;
  }

  const today = localIsoDate(new Date());
  contributionToConvert.value = contribution;
  newFocus.value = {
    ...createEmptyFocus(),
    assigneeId: contribution.assigneeId,
    description: contribution.description,
    endDate: contribution.dueDate && contribution.dueDate >= today ? contribution.dueDate : today,
    icon: contribution.icon,
    startDate: today,
    title: contribution.title,
  };
  focusDialog.value = true;
}
function canConvertToFocus(contribution: Contribution): boolean {
  return (
    contribution.status === "available" &&
    Boolean(
      contribution.assigneeId &&
      store.members.some((member) => member.id === contribution.assigneeId && member.role === "child"),
    )
  );
}
function closeFocusDialog() {
  focusDialog.value = false;
  contributionToConvert.value = undefined;
  newFocus.value = createEmptyFocus();
}
function openPreparationDialog(focusId: ContributionId) {
  selectedFocusId.value = focusId;
  preparationDates.value = [selectedFocus.value?.startDate ?? ""];
  preparationMinutes.value = 30;
  preparationReward.value = 1;
  preparationDialog.value = true;
}
async function savePreparations() {
  if (!selectedFocusId.value || !canSavePreparations.value) {
    return;
  }

  await store.addFocusPreparations(selectedFocusId.value, {
    dates: [...preparationDates.value],
    minutes: preparationMinutes.value,
    reward: preparationReward.value,
  });
  preparationDialog.value = false;
}
function isOverdue(dueDate?: string): boolean {
  return Boolean(dueDate && dueDate < today.value);
}
function requestOverdueResolution(item: Contribution | FocusGoal) {
  overdueResolutionTarget.value = {
    id: item.id,
    title: item.title,
    isFocus: "endDate" in item,
  };
}
async function confirmOverdueResolution() {
  const target = overdueResolutionTarget.value;

  if (!target) {
    return;
  }

  await store.resolveOverdueContribution(target.id);
  overdueResolutionTarget.value = undefined;
}
function localIsoDate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}
const calendarTasks = computed(() =>
  localizedContributions.value.filter(
    (contribution) =>
      (filter.value === "all" || contribution.category === filter.value) &&
      (personFilter.value === "all" || contribution.assigneeId === personFilter.value),
  ),
);
const calendarDateSlots = computed(() => {
  const today = new Date();
  const monday = new Date(today.getFullYear(), today.getMonth(), today.getDate());
  monday.setDate(monday.getDate() - ((monday.getDay() + 6) % 7) + weekOffset.value * 7);

  return Array.from({ length: calendarWeekSpan.value * 7 }, (_, index) => {
    const date = new Date(monday);
    date.setDate(monday.getDate() + index);
    const iso = localIsoDate(date);

    return {
      iso,
      weekday: new Intl.DateTimeFormat(locale.value, { weekday: "short" }).format(date),
      day: date.getDate(),
      isToday: iso === localIsoDate(today),
    };
  });
});
const calendarLegend = computed(() => {
  const children = store.members.filter(
    (member) =>
      member.role === "child" &&
      (store.viewerRole !== "child" || member.id === store.signedInMemberId) &&
      (personFilter.value === "all" || member.id === personFilter.value),
  );
  const members: Array<{
    appearance?: AvatarAppearance;
    avatar: string;
    color: string;
    memberId: string | undefined;
    taskCount: number;
    title: string;
  }> = children.map((member) => {
    const tasks = calendarTasks.value.filter(
      (contribution) =>
        contribution.assigneeId === member.id || (store.viewerRole === "child" && !contribution.assigneeId),
    );

    return {
      appearance: resolveFamilyMemberAvatarAppearance(member, store.members),
      avatar: member.avatar,
      color: member.color,
      memberId: member.id,
      taskCount: tasks.length,
      title: member.name,
    };
  });
  const openTasks = calendarTasks.value.filter((contribution) => !contribution.assigneeId);

  if (store.viewerRole !== "child" && openTasks.length) {
    members.push({
      avatar: "✨",
      color: familyMemberColorPalette.defaultGuardian,
      memberId: undefined,
      taskCount: openTasks.length,
      title: t("contributions.assignment.stillOpen"),
    });
  }

  return members;
});
const calendarDays = computed(() =>
  calendarDateSlots.value.map((day) => {
    const contributionEntries: CalendarEntry[] = calendarTasks.value
      .filter((contribution) => contribution.dueDate === day.iso)
      .map((contribution) => {
        const focus = visibleFocusGoals.value.find((item) => item.preparationContributionIds.includes(contribution.id));

        return {
          color:
            calendarLegend.value.find((member) => member.memberId === contribution.assigneeId)?.color ??
            familyMemberColorPalette.defaultGuardian,
          contribution,
          focus,
          focusLabel: focus ? `${t("contributions.calendar.focusPreparation")}: ${focus.title}` : undefined,
        };
      });
    const focusEntries: CalendarEntry[] = visibleFocusGoals.value
      .filter((focus) => focus.endDate === day.iso)
      .map((focus) => ({
        color:
          calendarLegend.value.find((member) => member.memberId === focus.assigneeId)?.color ??
          familyMemberColorPalette.defaultGuardian,
        focus,
        focusLabel: t("contributions.calendar.focusGoal"),
      }));

    return {
      ...day,
      entries: [...focusEntries, ...contributionEntries],
    };
  }),
);
const calendarRangeLabel = computed(() => {
  const first = calendarDateSlots.value.at(0)?.iso;
  const last = calendarDateSlots.value.at(-1)?.iso;
  const formatter = new Intl.DateTimeFormat(locale.value, { day: "2-digit", month: "short", year: "numeric" });

  return first && last
    ? `${formatter.format(new Date(`${first}T12:00:00`))} – ${formatter.format(new Date(`${last}T12:00:00`))}`
    : "";
});
async function showCurrentWeek() {
  calendarExpanded.value = true;
  weekOffset.value = 0;
  await nextTick();
  document
    .querySelector(".week-day--today")
    ?.scrollIntoView({ behavior: "smooth", block: "nearest", inline: "center" });
}
function openCalendarEntry(entry: CalendarEntry) {
  if (entry.contribution) {
    selectedCalendarContribution.value = entry.contribution;

    return;
  }

  if (entry.focus) {
    const focusCard = document.getElementById(`focus-${entry.focus.id}`);
    focusCard?.scrollIntoView({ behavior: "smooth", block: "center" });
    focusCard?.focus({ preventScroll: true });
  }
}
const contributionDialogTitle = computed(() =>
  t(contributionToEdit.value ? "contributions.form.editTitle" : "contributions.form.title"),
);
const contributionSubmitLabel = computed(() => t(contributionToEdit.value ? "common.save" : "common.add"));
const recurrenceOptions = computed(() => [
  { title: t("contributions.recurrence.none"), value: "none" },
  { title: t("contributions.recurrence.daily"), value: "daily" },
  { title: t("contributions.recurrence.weekly"), value: "weekly" },
]);
const emptyStateIsOpen = computed(() => statusFilter.value === "open");
const emptyStateTitle = computed(() =>
  t(emptyStateIsOpen.value ? "contributions.empty.openTitle" : "contributions.empty.completedTitle"),
);
const emptyStateDescription = computed(() =>
  t(emptyStateIsOpen.value ? "contributions.empty.openDescription" : "contributions.empty.completedDescription"),
);
const emptyStateActionLabel = computed(() =>
  t(emptyStateIsOpen.value ? "contributions.empty.viewCompleted" : "contributions.empty.viewOpen"),
);
const contributionDeleteWarning = computed(() =>
  t(
    contributionToDelete.value?.status === "approved"
      ? "contributions.deleteContribution.approved"
      : "contributions.deleteContribution.open",
  ),
);
const childReviewCards = computed(() =>
  guardianTasksToRate.value.map((contribution) => ({
    contribution,
    completedByLabel: t("contributions.completedBy", { name: memberName(contribution.assigneeId) }),
  })),
);
const showReviewableTasks = computed(
  () =>
    (store.permissions.canManageContent || store.permissions.canReviewChildTasks) && reviewableTasks.value.length > 0,
);
const reviewableTaskCards = computed(() =>
  reviewableTasks.value.map((contribution) => ({
    contribution,
    metaLabel: `${memberName(contribution.assigneeId)} · +${t("contributions.reward.coins", { value: store.rewardForContribution(contribution.id) })}`,
  })),
);
const canSaveContribution = computed(() =>
  Boolean(
    newContribution.description.trim() &&
    newContribution.category.trim() &&
    newContribution.icon.trim() &&
    newContribution.title.trim() &&
    newContribution.reward >= 0 &&
    Number.isInteger(newContribution.quantity) &&
    newContribution.quantity >= 1 &&
    newContribution.quantity <= 20 &&
    (newContribution.recurrence === "none" || Boolean(newContribution.dueDate)) &&
    (newContribution.kind === "extra" || newContribution.energy >= 1),
  ),
);
const canStartPromotion = computed(() => Boolean(newPromotion.contributionId && newPromotion.deadline));
function toggleEmptyStateFilter() {
  statusFilter.value = emptyStateIsOpen.value ? "completed" : "open";
  filter.value = "all";
}
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.empty-contributions {
  --uno: d-flex flex-column align-center;
  border: 2px dashed color-mix(in srgb, var(--lad-color-primary) 30%, transparent);
  background:
    radial-gradient(
      circle at 80% 15%,
      color-mix(in srgb, var(--lad-color-reward-highlight) 20%, transparent),
      transparent 27%
    ),
    linear-gradient(145deg, var(--lad-surface-soft), var(--lad-surface));
  box-shadow: inset 0 0 0 5px color-mix(in srgb, var(--lad-surface-raised) 40%, transparent);
}

.contribution-card-list {
  padding-inline: rem(17);
}

.contribution-card-grid,
.managed-contribution-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(100%, rem(350)), 1fr));
  align-items: stretch;
  gap: 12px;
}

.schedule-and-focus {
  display: flex;
  flex-direction: column;
}

.schedule-and-focus .task-calendar {
  order: 1;
}

.schedule-and-focus .focus-goals {
  order: 2;
}

.task-calendar {
  padding: 16px;
  border: 1px solid var(--lad-border);
  border-radius: 20px;
  background:
    radial-gradient(
      circle at 96% 8%,
      color-mix(in srgb, var(--lad-color-primary) 12%, transparent) 0 36px,
      transparent 37px
    ),
    linear-gradient(145deg, var(--lad-surface), var(--lad-surface-soft));
  box-shadow: 0 6px 0 color-mix(in srgb, var(--lad-color-primary) 10%, transparent);
}

.calendar-toolbar strong {
  font-size: rem(17);
  letter-spacing: -0.02em;
}

.calendar-toolbar strong::before {
  display: inline-grid;
  width: 29px;
  height: 29px;
  margin-inline-end: 7px;
  place-items: center;
  content: "🗓️";
  border-radius: 10px;
  background: color-mix(in srgb, var(--lad-color-primary) 12%, var(--lad-surface-raised));
  box-shadow: 0 2px 0 color-mix(in srgb, var(--lad-color-primary) 18%, transparent);
  vertical-align: middle;
}

.week-grid {
  width: 100%;
  max-width: 100%;
  min-width: 0;
  display: grid;
  grid-template-columns: repeat(7, minmax(110px, 1fr));
  gap: 8px;
  overflow-x: auto;
  overscroll-behavior-inline: contain;
  -webkit-overflow-scrolling: touch;
}

.calendar-legend {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 8px;
}

.calendar-legend__member {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 5px 9px 5px 5px;
  border: 1px solid var(--lad-border);
  border-radius: var(--lad-radius-pill);
  background: var(--lad-surface-soft);
  font-size: rem(11);
  font-weight: 800;
}

.calendar-member-avatar {
  width: 36px;
  height: 36px;
  display: grid;
  flex: 0 0 36px;
  place-items: center;
  overflow: hidden;
  border: 2px solid var(--lad-surface-raised);
  border-radius: 50%;
  background: var(--lad-color-primary-soft);
  box-shadow: 0 2px 0 color-mix(in srgb, var(--lad-color-primary) 24%, transparent);
}

.week-day {
  min-height: 130px;
  padding: 8px;
  border: 1px solid var(--lad-border);
  border-radius: 14px;
  background: var(--lad-surface-soft);
}

.week-day--today {
  border-color: var(--lad-color-primary);
  box-shadow: inset 0 0 0 1px var(--lad-color-primary);
}

.week-day__heading {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
  font-size: 0.75rem;
}

.week-day__empty {
  color: var(--lad-muted);
}

.calendar-task {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 5px;
  margin-bottom: 6px;
  padding: 7px;
  border: 0;
  border-radius: 10px;
  border-inline-start: 4px solid var(--calendar-task-color);
  background: color-mix(in srgb, var(--calendar-task-color) 16%, var(--lad-surface));
  color: inherit;
  text-align: left;
  cursor: pointer;
}

.calendar-task--focus-goal {
  border: 1px solid var(--calendar-task-color);
  border-inline-start-width: 4px;
  background: color-mix(in srgb, var(--calendar-task-color) 26%, var(--lad-surface));
}

.calendar-task--focus-preparation {
  border-block: 1px dashed color-mix(in srgb, var(--calendar-task-color) 55%, transparent);
  border-inline-end: 1px dashed color-mix(in srgb, var(--calendar-task-color) 55%, transparent);
}

.calendar-task--completed {
  background: var(--lad-gradient-success);
  opacity: 0.82;
}

.calendar-task--completed .calendar-task__title {
  text-decoration: line-through;
}

.calendar-task__copy {
  min-width: 0;
  flex: 1;
}

.calendar-task__copy small {
  display: block;
  overflow: hidden;
  color: var(--lad-muted);
  font-size: rem(8);
  font-weight: 700;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.calendar-task__title {
  display: block;
  overflow: hidden;
  font-size: 0.72rem;
  font-weight: 700;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.management-member-filter {
  max-width: 28rem;
  margin-inline: rem(17);
}

.guardian-context {
  border: 1px solid color-mix(in srgb, var(--lad-color-info) 20%, transparent);
}

.review-card,
.basic-row {
  border: 1px solid var(--lad-border);
  box-shadow: 0 4px 0 var(--lad-border);
}

.review-card-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(100%, rem(350)), 1fr));
  align-items: stretch;
  gap: 12px;
}

.review-card {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.review-card__assessment {
  margin-top: auto;
}

.task-icon-avatar {
  @include task-icon-tile;
}

.task-icon-avatar--compact {
  flex-basis: 50px;
  font-size: rem(27);
}

.task-icon-avatar--small {
  flex-basis: 44px;
  font-size: 1.5rem;
}

.finish-button {
  @include action-button;
}

.finish-check {
  @include action-button-icon;
}

.promotion-row {
  border: 1px solid color-mix(in srgb, var(--lad-color-reward-border) 25%, transparent);
  --uno: cursor-pointer;
}

.promotion-detail-icon {
  width: 62px;
  height: 62px;
  --uno: d-grid place-center;
  border-radius: 20px;
  background: var(--lad-color-reward-pale);
  font-size: 2rem;
}

.promotion-detail-title {
  --uno: ma-0;
  font-size: rem(22);
  letter-spacing: -0.03em;
}

.promotion-reward {
  padding: 14px;
  border-radius: 16px;
  background: var(--lad-color-reward-soft);
}

.promotion-reward span,
.promotion-reward strong,
.promotion-reward small {
  --uno: d-block;
}

.promotion-reward span,
.promotion-reward small {
  color: var(--lad-muted);
  font-size: rem(11);
}

.promotion-reward strong {
  color: var(--lad-color-reward-ink);
  font-size: rem(27);
}

.team-dialog-icon {
  width: 58px;
  height: 58px;
  --uno: d-grid place-center;
  border-radius: 18px;
  background: var(--lad-surface-soft);
  font-size: rem(30);
}

.partner-rule {
  padding: 10px 12px;
  --uno: d-flex align-start ga-2;
  color: var(--lad-muted);
  border-radius: 12px;
  background: var(--lad-surface-soft);
  font-size: rem(11);
  line-height: 1.4;
}

.focus-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(min(100%, 320px), 1fr));
  gap: 12px;
}

.focus-goals :deep(.family-world-section-header) {
  padding: 12px 14px;
  border: 1px solid color-mix(in srgb, var(--lad-color-reward-border) 22%, transparent);
  border-radius: 18px;
  background:
    radial-gradient(
      circle at 94% 10%,
      color-mix(in srgb, var(--lad-color-reward) 16%, transparent) 0 28px,
      transparent 29px
    ),
    linear-gradient(145deg, var(--lad-surface-raised), var(--lad-color-reward-soft));
  box-shadow: 0 4px 0 color-mix(in srgb, var(--lad-color-reward-border) 14%, transparent);
}

.focus-goals :deep(.section-copy h2)::before {
  display: inline-block;
  margin-inline-end: 7px;
  content: "🎯";
}

.focus-card {
  border: 1px solid var(--lad-border);
}

.focus-card:focus {
  outline: 3px solid color-mix(in srgb, var(--lad-color-info) 42%, transparent);
  outline-offset: 3px;
}

.focus-target-badge {
  position: relative;
  width: 52px;
  height: 52px;
  display: grid;
  flex: 0 0 52px;
  place-items: center;
  overflow: hidden;
  color: var(--lad-color-reward-ink);
  border: 2px solid color-mix(in srgb, var(--lad-color-reward-border) 34%, transparent);
  border-radius: 17px;
  background: linear-gradient(145deg, var(--lad-color-reward-pale), var(--lad-surface-raised));
  box-shadow: 0 4px 0 color-mix(in srgb, var(--lad-color-reward-border) 22%, transparent);
}

.focus-target-badge__ring,
.focus-target-badge__dot {
  position: absolute;
  border-radius: 50%;
}

.focus-target-badge__ring {
  width: 30px;
  height: 30px;
  border: 4px solid var(--lad-color-reward);
  box-shadow: inset 0 0 0 5px var(--lad-surface-raised);
  animation: focus-target-pulse 2.4s ease-in-out infinite;
}

.focus-target-badge__dot {
  width: 7px;
  height: 7px;
  background: var(--lad-color-reward-ink);
}

.focus-target-badge .v-icon {
  position: absolute;
  top: 5px;
  right: 4px;
  filter: drop-shadow(0 2px 0 var(--lad-surface-raised));
  transform: rotate(-4deg);
  animation: focus-arrow-float 2.4s ease-in-out infinite;
}

.focus-owner-badge {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 2px 8px 2px 3px;
  color: var(--lad-color-primary-deep);
  border: 1px solid color-mix(in srgb, var(--lad-color-info) 25%, transparent);
  border-radius: var(--lad-radius-pill);
  background: color-mix(in srgb, var(--lad-color-info) 10%, var(--lad-surface-raised));
  font-size: rem(10);
  font-weight: 800;
}

.focus-owner-badge__avatar {
  width: 24px;
  height: 24px;
  display: grid;
  place-items: center;
  overflow: hidden;
  border-radius: 50%;
  background: var(--lad-surface-raised);
}

.focus-kind-badge__target {
  display: inline-block;
  margin-inline-end: 3px;
  animation: focus-target-wiggle 3s ease-in-out infinite;
}

.focus-deadline {
  width: fit-content;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 7px 10px 7px 7px;
  color: var(--lad-color-primary-deep);
  border: 1px solid color-mix(in srgb, var(--lad-color-primary) 24%, transparent);
  border-radius: 14px;
  background: color-mix(in srgb, var(--lad-color-primary) 9%, var(--lad-surface-raised));
}

.focus-deadline__icon {
  width: 29px;
  height: 29px;
  display: grid;
  place-items: center;
  border-radius: 10px;
  background: var(--lad-surface-raised);
  box-shadow: 0 2px 0 color-mix(in srgb, var(--lad-color-primary) 18%, transparent);
}

.focus-deadline strong,
.focus-deadline small {
  display: block;
  line-height: 1.2;
}

.focus-deadline strong {
  font-size: rem(12);
}

.focus-deadline small {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: rem(9);
  font-weight: 700;
}

@keyframes focus-target-pulse {
  0%,
  100% {
    transform: scale(0.94);
  }
  50% {
    transform: scale(1.05);
  }
}

@keyframes focus-arrow-float {
  0%,
  100% {
    transform: translate(0, 0) rotate(-4deg);
  }
  50% {
    transform: translate(2px, -2px) rotate(-4deg);
  }
}

@keyframes focus-target-wiggle {
  0%,
  78%,
  100% {
    transform: rotate(0deg) scale(1);
  }
  84% {
    transform: rotate(-9deg) scale(1.08);
  }
  90% {
    transform: rotate(8deg) scale(1.08);
  }
}

@media (prefers-reduced-motion: reduce) {
  .focus-target-badge__ring,
  .focus-target-badge .v-icon,
  .focus-kind-badge__target {
    animation: none;
  }
}

.section-title {
  @include section-title;
}

.review-actions {
  grid-template-columns: 0.8fr 1.2fr;
}

.empty-icon {
  font-size: rem(42);
}

.form-columns {
  grid-template-columns: 1fr 1fr;
}

.assignment-select {
  min-width: 0;
  font-size: rem(10);
}

.management-actions {
  width: 100%;
  --uno: d-grid align-center;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 8px;
}

.management-action-buttons {
  --uno: d-flex align-center flex-wrap justify-end;
  gap: 6px;
}

.list-enter-active,
.list-leave-active {
  transition:
    opacity 200ms ease,
    transform 200ms ease;
}

.list-enter-from,
.list-leave-to {
  opacity: 0;
  transform: translateY(8px);
}

@keyframes task-icon-float {
  0%,
  68%,
  100% {
    transform: translateY(0) rotate(-3deg);
  }

  78% {
    transform: translateY(-3px) rotate(3deg) scale(1.04);
  }

  88% {
    transform: translateY(1px) rotate(-1deg);
  }
}

@include reduced-motion {
  .task-icon-avatar {
    animation: none;
  }
}

@include respond-down(shell) {
  .week-grid:not(.week-grid--multiweek) {
    grid-template-columns: none;
    grid-auto-columns: calc(33.333333% - 5.333333px);
    grid-auto-flow: column;
    padding-bottom: 8px;
    overflow-x: scroll;
    overflow-y: hidden;
    scroll-snap-type: inline mandatory;
    touch-action: pan-x;
  }

  .week-day {
    scroll-snap-align: start;
    scroll-snap-stop: always;
  }

  .week-grid--multiweek {
    grid-template-columns: repeat(7, minmax(0, 1fr));
    gap: 3px;
    overflow: visible;
  }

  .week-grid--multiweek .week-day {
    min-width: 0;
    min-height: 76px;
    padding: 4px 2px;
    border-radius: 8px;
    scroll-snap-align: none;
  }

  .week-grid--multiweek .week-day__heading {
    margin-bottom: 4px;
    font-size: rem(9);
  }

  .week-grid--multiweek .calendar-task {
    min-width: 0;
    gap: 2px;
    margin-bottom: 3px;
    padding: 3px 2px;
    border-inline-start-width: 2px;
    border-radius: 5px;
    font-size: rem(8);
  }

  .week-grid--multiweek .calendar-task > :first-child:not(.calendar-task__copy),
  .week-grid--multiweek .calendar-task > .v-icon:last-child,
  .week-grid--multiweek .calendar-task__copy small {
    display: none;
  }

  .week-grid--multiweek .calendar-task__title {
    display: block;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
}

@include respond-down(compact) {
  .form-columns {
    grid-template-columns: 1fr;
  }

  .management-actions {
    grid-template-columns: minmax(0, 1fr);
  }

  .management-action-buttons {
    justify-content: flex-start;
  }
}
</style>
