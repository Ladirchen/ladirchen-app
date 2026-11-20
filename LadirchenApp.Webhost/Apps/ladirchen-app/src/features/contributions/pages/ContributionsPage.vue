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

    <section class="focus-goals mb-6">
      <SectionHeader :description="t('contributions.focus.description')" :title="t('contributions.focus.title')">
        <template v-if="store.permissions.canManageContent || store.permissions.canManageChildTasks" #action>
          <v-btn
            color="primary"
            prepend-icon="i-mdi:target"
            rounded="lg"
            size="small"
            variant="tonal"
            @click="focusDialog = true"
          >
            {{ t("contributions.focus.add") }}
          </v-btn>
        </template>
      </SectionHeader>
      <div v-if="visibleFocusGoals.length" class="focus-grid">
        <BrandedCard v-for="focus in visibleFocusGoals" :key="focus.id" class="focus-card pa-4" tone="contributions">
          <div class="d-flex align-start ga-3">
            <v-avatar color="warning" rounded="lg" size="52" variant="tonal">{{ focus.icon }}</v-avatar>
            <div class="flex-grow-1 min-w-0">
              <div class="d-flex align-center justify-space-between ga-2">
                <strong>{{ focus.title }}</strong>
                <v-chip color="warning" size="x-small" variant="tonal">{{ focusPeriodLabel(focus.period) }}</v-chip>
              </div>
              <p class="text-caption text-medium-emphasis">{{ focus.description }}</p>
              <p class="text-caption mt-1">
                <v-icon icon="i-mdi:timer-sand" size="15" /> {{ focusCountdown(focus.endDate) }}
              </p>
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
        </BrandedCard>
      </div>
      <p v-else class="text-caption text-medium-emphasis">{{ t("contributions.focus.empty") }}</p>
    </section>

    <section class="task-calendar mb-6">
      <div class="d-flex align-center justify-space-between ga-3 mb-3">
        <div>
          <strong>{{ t("contributions.calendar.title") }}</strong>
          <p class="text-caption text-medium-emphasis">{{ calendarRangeLabel }}</p>
        </div>
        <div class="d-flex align-center ga-1">
          <v-btn
            :aria-label="t('contributions.calendar.previous')"
            icon="i-mdi:chevron-left"
            size="small"
            variant="text"
            @click="weekOffset -= 1"
          />
          <v-btn size="small" variant="tonal" @click="weekOffset = 0">{{ t("contributions.calendar.today") }}</v-btn>
          <v-btn
            :aria-label="t('contributions.calendar.next')"
            icon="i-mdi:chevron-right"
            size="small"
            variant="text"
            @click="weekOffset += 1"
          />
        </div>
      </div>
      <div class="calendar-member-groups">
        <section v-for="group in calendarGroups" :key="group.memberId ?? 'open'" class="calendar-member-group">
          <div class="calendar-member-heading">
            <v-avatar :color="group.color" size="30" variant="tonal">{{ group.avatar }}</v-avatar>
            <strong>{{ group.title }}</strong>
            <v-chip size="x-small" variant="tonal">{{ group.taskCount }}</v-chip>
          </div>
          <div class="week-grid">
            <div v-for="day in group.days" :key="day.iso" class="week-day" :class="{ 'week-day--today': day.isToday }">
              <div class="week-day__heading">
                <span>{{ day.weekday }}</span>
                <strong>{{ day.day }}</strong>
              </div>
              <button
                v-for="contribution in day.contributions"
                :key="contribution.id"
                class="calendar-task"
                :class="`calendar-task--${contribution.kind}`"
                type="button"
                @click="openCalendarTask(contribution)"
              >
                <SchabiTaskIcon v-if="isSchabiContributionId(contribution.id)" />
                <span v-else>{{ contribution.icon }}</span>
                <span class="calendar-task__title">{{ contribution.title }}</span>
                <v-icon v-if="contribution.recurrence !== 'none'" icon="i-mdi:repeat" size="14" />
              </button>
              <span v-if="!day.contributions.length" class="week-day__empty">—</span>
            </div>
          </div>
        </section>
        <p v-if="!calendarGroups.length" class="text-caption text-medium-emphasis">
          {{ t("contributions.calendar.empty") }}
        </p>
      </div>
    </section>

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
        <div class="d-flex flex-column ga-3">
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
        :open-count="openContributionCount"
        :person-options="personFilterOptions"
        :show-person="false"
        :show-status="false"
      />

      <TransitionGroup class="contribution-card-list d-flex flex-column ga-3" name="list" tag="div">
        <ChildContributionCard
          v-for="contribution in filteredContributions"
          :key="contribution.id"
          :active-child-id="store.activeChildId"
          :contribution="contribution"
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

      <section v-if="store.signedInMember?.guardianAccess !== 'external'" class="guardian-own-tasks mb-6">
        <SectionHeader
          :description="t('contributions.guardianOwn.description')"
          :title="t('contributions.guardianOwn.title')"
        >
          <template #action>
            <v-btn
              color="secondary"
              prepend-icon="i-mdi:plus"
              rounded="lg"
              size="small"
              variant="tonal"
              @click="openOwnTaskDialog"
            >
              {{ t("contributions.guardianOwn.add") }}
            </v-btn>
          </template>
        </SectionHeader>
        <div v-if="guardianOwnContributions.length" class="d-flex flex-column ga-2">
          <BrandedCard
            v-for="{ contribution, statusDescription, statusColor, statusLabel } in guardianOwnCards"
            :key="contribution.id"
            class="basic-row pa-3"
            tone="contributions"
          >
            <div class="d-flex align-center ga-3">
              <v-avatar class="task-icon-avatar task-icon-avatar--small" color="surface-variant" rounded="lg" size="44">
                {{ contribution.icon }}
              </v-avatar>
              <div class="flex-grow-1 min-w-0">
                <strong class="text-body-small">{{ contribution.title }}</strong>
                <ContributionDescription :text="contribution.description" />
                <p class="text-caption text-medium-emphasis">{{ statusDescription }}</p>
              </div>
              <v-btn
                v-if="contribution.status === 'available'"
                class="finish-button guardian-finish-button"
                color="info"
                rounded="lg"
                size="small"
                variant="flat"
                @click="store.submitContribution(contribution.id)"
              >
                <span class="finish-check" aria-hidden="true"><v-icon icon="i-mdi:check" size="24" /></span>
                <span>{{ t("contributions.finish") }}</span>
              </v-btn>
              <v-chip v-else :color="statusColor" size="small" variant="tonal">{{ statusLabel }}</v-chip>
            </div>
          </BrandedCard>
        </div>
        <BrandedCard v-else class="empty-review pa-4 text-center" tone="contributions">
          <p class="text-caption text-medium-emphasis">{{ t("contributions.guardianOwn.empty") }}</p>
        </BrandedCard>
      </section>

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
            <div class="d-flex align-center justify-space-between ga-2 mt-3">
              <p class="text-caption text-medium-emphasis">{{ t("contributions.review.question") }}</p>
              <v-chip color="warning" size="x-small" variant="tonal">
                5 ★ = +{{ store.perfectRatingBonusPercent }} %
              </v-chip>
            </div>
            <v-rating v-model="ratings[contribution.id]" active-color="warning" class="my-2" density="compact" hover />
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
                  icon="i-mdi:delete-outline"
                  size="small"
                  variant="tonal"
                  @click.stop="promotionToDelete = promotion"
                />
              </div>
            </BrandedCard>
          </div>

          <BrandedCard class="rating-setting pa-4 mb-6" tone="contributions">
            <div class="d-flex align-start ga-3">
              <v-avatar color="warning" variant="tonal">★</v-avatar>
              <div class="flex-grow-1">
                <div class="d-flex align-center justify-space-between ga-3">
                  <div>
                    <strong>{{ t("contributions.ratingBonus.title") }}</strong>
                    <p class="text-caption text-medium-emphasis">{{ t("contributions.ratingBonus.description") }}</p>
                  </div>
                  <strong class="bonus-value">{{ store.perfectRatingBonusPercent }} %</strong>
                </div>
                <v-slider
                  :model-value="store.perfectRatingBonusPercent"
                  class="mt-3"
                  color="warning"
                  hide-details
                  max="25"
                  min="0"
                  step="1"
                  thumb-label
                  @end="store.setPerfectRatingBonusPercent(Number($event))"
                />
              </div>
            </div>
          </BrandedCard>
        </template>

        <SectionHeader :description="t('contributions.manage.description')" :title="t('contributions.manage.title')">
          <template #action>
            <v-btn color="primary" icon="i-mdi:plus" size="small" variant="tonal" @click="openContributionDialog" />
          </template>
        </SectionHeader>
        <v-select
          v-model="managedMemberFilter"
          class="management-member-filter mb-4"
          density="comfortable"
          hide-details
          :items="managedMemberOptions"
          item-title="title"
          item-value="value"
          :label="t('contributions.manage.filterLabel')"
          variant="outlined"
        />
        <div class="contribution-card-list d-flex flex-column ga-5">
          <section v-for="group in managedContributionGroups" :key="group.memberId ?? 'open'">
            <div class="member-task-heading d-flex align-center ga-2 mb-2">
              <v-avatar color="primary" size="30" variant="tonal">
                {{ store.members.find((member) => member.id === group.memberId)?.avatar ?? "✨" }}
              </v-avatar>
              <strong>{{ group.title }}</strong>
              <v-chip size="x-small" variant="tonal">{{ group.contributions.length }}</v-chip>
            </div>
            <div class="d-flex flex-column ga-3">
              <ChildContributionCard
                v-for="contribution in group.contributions"
                :key="contribution.id"
                :active-child-id="store.activeChildId"
                :allow-invite="false"
                :contribution="contribution"
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
                        icon="i-mdi:restore"
                        size="small"
                        variant="tonal"
                        @click="reactivateContribution(contribution)"
                      />
                      <v-btn
                        v-else
                        :aria-label="t('contributions.manage.copyAria', { title: contribution.title })"
                        icon="i-mdi:content-copy"
                        size="small"
                        variant="tonal"
                        @click="copyContribution(contribution)"
                      />
                      <v-btn
                        v-if="contribution.status === 'available'"
                        :aria-label="t('contributions.manage.editAria', { title: contribution.title })"
                        icon="i-mdi:pencil-outline"
                        size="small"
                        variant="tonal"
                        @click="openContributionEditor(contribution)"
                      />
                      <v-btn
                        :aria-label="t('contributions.manage.deleteAria', { title: contribution.title })"
                        color="error"
                        icon="i-mdi:delete-outline"
                        size="small"
                        variant="tonal"
                        @click="requestContributionDelete(contribution)"
                      />
                    </div>
                  </div>
                </template>
              </ChildContributionCard>
            </div>
          </section>
        </div>
      </template>
    </template>

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
        <v-card-title class="pa-0 mb-1">{{ t("contributions.focus.formTitle") }}</v-card-title>
        <v-card-subtitle class="pa-0 mb-4">{{ t("contributions.focus.formDescription") }}</v-card-subtitle>
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
          <v-btn rounded="lg" variant="text" @click="focusDialog = false">{{ t("common.cancel") }}</v-btn>
          <v-btn color="primary" :disabled="!canSaveFocus" rounded="lg" variant="flat" @click="saveFocus">
            {{ t("common.add") }}
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
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import type { Contribution, FocusPeriod, NewFocusGoal } from "@/domain/contributions/types";
import type { ContributionId, FamilyMemberId } from "@/domain/shared/identifiers";
import { isSchabiContributionId } from "@/domain/contributions/source";
import AnimatedCompletionMark from "@/shared/components/AnimatedCompletionMark.vue";
import ChildContributionCard from "@/shared/components/contributions/ChildContributionCard.vue";
import ContributionDescription from "@/shared/components/contributions/ContributionDescription.vue";
import SchabiTaskIcon from "@/shared/components/contributions/SchabiTaskIcon.vue";
import ContributionFilterPanel from "@/features/contributions/components/ContributionFilterPanel.vue";
import PromotionCountdown from "@/shared/components/contributions/PromotionCountdown.vue";
import SectionHeader from "@/shared/components/ui/SectionHeader.vue";
import BrandedCard from "@/shared/components/ui/BrandedCard.vue";
import { useContributionsPage } from "@/features/contributions/composables/use-contributions-page";
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
  guardianContributionStatus,
  guardianOwnContributions,
  guardianTasksToRate,
  kindOptions,
  localizedContributions,
  managedContributionGroups,
  managedMemberFilter,
  managedMemberOptions,
  memberName,
  multiplierOptions,
  newContribution,
  newPromotion,
  openContributionCount,
  openContributionDialog,
  openContributionEditor,
  openOwnTaskDialog,
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
const preparationDates = ref([""]);
const preparationMinutes = ref(30);
const preparationReward = ref(1);
const newFocus = ref<NewFocusGoal>(createEmptyFocus());
const weekOffset = ref(0);
const visibleFocusGoals = computed(() =>
  store.focusGoals.filter((focus) => store.viewerRole !== "child" || focus.assigneeId === store.signedInMemberId),
);
const selectedFocus = computed(() => store.focusGoals.find((focus) => focus.id === selectedFocusId.value));
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
function focusCountdown(endDate: string): string {
  const end = new Date(`${endDate}T23:59:59`).getTime();
  const days = Math.max(0, Math.ceil((end - store.currentTimeMilliseconds) / 86_400_000));

  return t("contributions.focus.countdown", { days });
}
async function saveFocus() {
  if (!canSaveFocus.value) {
    return;
  }

  await store.addFocus({ ...newFocus.value });
  focusDialog.value = false;
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
function localIsoDate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}
const calendarTasks = computed(() =>
  localizedContributions.value.filter((contribution) => contribution.status !== "approved"),
);
const calendarDateSlots = computed(() => {
  const today = new Date();
  const monday = new Date(today.getFullYear(), today.getMonth(), today.getDate());
  monday.setDate(monday.getDate() - ((monday.getDay() + 6) % 7) + weekOffset.value * 7);

  return Array.from({ length: 7 }, (_, index) => {
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
const calendarGroups = computed(() => {
  const children = store.members.filter(
    (member) => member.role === "child" && (store.viewerRole !== "child" || member.id === store.signedInMemberId),
  );
  const groups: Array<{
    avatar: string;
    color: string;
    days: Array<(typeof calendarDateSlots.value)[number] & { contributions: Contribution[] }>;
    memberId: string | undefined;
    taskCount: number;
    title: string;
  }> = children.map((member) => {
    const tasks = calendarTasks.value.filter(
      (contribution) =>
        contribution.assigneeId === member.id || (store.viewerRole === "child" && !contribution.assigneeId),
    );

    return {
      avatar: member.avatar,
      color: member.color,
      days: calendarDateSlots.value.map((day) => ({
        ...day,
        contributions: tasks.filter((contribution) => contribution.dueDate === day.iso),
      })),
      memberId: member.id,
      taskCount: tasks.length,
      title: member.name,
    };
  });
  const openTasks = calendarTasks.value.filter((contribution) => !contribution.assigneeId);

  if (store.viewerRole !== "child" && openTasks.length) {
    groups.push({
      avatar: "✨",
      color: "primary",
      days: calendarDateSlots.value.map((day) => ({
        ...day,
        contributions: openTasks.filter((contribution) => contribution.dueDate === day.iso),
      })),
      memberId: undefined,
      taskCount: openTasks.length,
      title: t("contributions.assignment.stillOpen"),
    });
  }

  return groups;
});
const calendarRangeLabel = computed(() => {
  const first = calendarDateSlots.value.at(0)?.iso;
  const last = calendarDateSlots.value.at(-1)?.iso;
  const formatter = new Intl.DateTimeFormat(locale.value, { day: "2-digit", month: "short", year: "numeric" });

  return first && last
    ? `${formatter.format(new Date(`${first}T12:00:00`))} – ${formatter.format(new Date(`${last}T12:00:00`))}`
    : "";
});
function openCalendarTask(contribution: Contribution) {
  if (store.viewerRole !== "child" && contribution.status === "available") {
    openContributionEditor(contribution);
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
const guardianOwnCards = computed(() =>
  guardianOwnContributions.value.map(function callback(contribution) {
    const approved = contribution.status === "approved";

    return {
      contribution,
      statusDescription: guardianContributionStatus(contribution),
      statusColor: approved ? "success" : "warning",
      statusLabel: t(approved ? "contributions.guardianOwn.rated" : "contributions.guardianOwn.submitted"),
    };
  }),
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

.task-calendar {
  padding: 16px;
  border: 1px solid var(--lad-border);
  border-radius: 20px;
  background: var(--lad-surface);
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

.calendar-member-groups {
  min-width: 0;
  display: grid;
  gap: 20px;
}

.calendar-member-group {
  min-width: 0;
}

.calendar-member-group + .calendar-member-group {
  padding-top: 18px;
  border-top: 1px solid var(--lad-border);
}

.calendar-member-heading {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 10px;
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
  background: color-mix(in srgb, var(--lad-color-primary) 12%, var(--lad-surface));
  color: inherit;
  text-align: left;
  cursor: pointer;
}

.calendar-task--school {
  background: color-mix(in srgb, var(--lad-color-info) 16%, var(--lad-surface));
}

.calendar-task__title {
  min-width: 0;
  flex: 1;
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

.rating-setting {
  border: 1px solid var(--lad-border);
}

.focus-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(min(100%, 320px), 1fr));
  gap: 12px;
}

.focus-card {
  border: 1px solid var(--lad-border);
}

.bonus-value {
  color: var(--lad-color-reward-deep);
  font-size: rem(18);
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
  .week-grid {
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
