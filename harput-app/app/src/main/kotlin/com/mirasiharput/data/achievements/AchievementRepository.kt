package com.mirasiharput.data.achievements

import android.content.Context
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.intPreferencesKey
import androidx.datastore.preferences.core.stringSetPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import com.mirasiharput.features.quiz.QuizRepository
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map

private val Context.achievementDataStore by preferencesDataStore(name = "achievements")

/**
 * Yarışma sonucu kazanılan puan ve rozet özeti.
 */
data class QuizAward(
    val score: Int,
    val correctCount: Int,
    val isNewBest: Boolean,
    val totalPoints: Int,
    val newBadges: List<Badge>,
)

/**
 * Miras Puanı ve rozetleri cihazda kalıcı olarak saklar.
 *
 * Puanlama: her doğru cevap 10 puan (yarışma başına en fazla 100).
 * Toplam Miras Puanı, her mekanın EN İYİ skorunun toplamıdır; böylece
 * yarışma tekrarlanarak puan şişirilemez, ancak skor her zaman geliştirilebilir.
 */
class AchievementRepository(context: Context) {

    private val appContext = context.applicationContext
    private val earnedBadgesKey = stringSetPreferencesKey("earned_badge_ids")

    private fun bestScoreKey(locationId: String) = intPreferencesKey("best_score_$locationId")

    val totalPoints: Flow<Int> = appContext.achievementDataStore.data
        .map { prefs ->
            TRACKED_LOCATION_IDS.sumOf { prefs[bestScoreKey(it)] ?: 0 }
        }

    val earnedBadgeIds: Flow<Set<String>> = appContext.achievementDataStore.data
        .map { prefs -> prefs[earnedBadgesKey] ?: emptySet() }

    suspend fun recordQuizResult(locationId: String, correctCount: Int): QuizAward {
        val score = correctCount * QuizRepository.POINTS_PER_CORRECT_ANSWER
        var isNewBest = false
        var newBadges = emptyList<Badge>()

        appContext.achievementDataStore.edit { prefs ->
            val previousBest = prefs[bestScoreKey(locationId)] ?: 0
            if (score > previousBest) {
                prefs[bestScoreKey(locationId)] = score
                isNewBest = true
            }

            val bestScores = TRACKED_LOCATION_IDS.associateWith { id ->
                prefs[bestScoreKey(id)] ?: 0
            }
            val alreadyEarned = prefs[earnedBadgesKey] ?: emptySet()
            val deserved = evaluateBadges(bestScores)
            val newlyEarned = deserved - alreadyEarned
            if (newlyEarned.isNotEmpty()) {
                prefs[earnedBadgesKey] = alreadyEarned + newlyEarned
            }
            newBadges = newlyEarned.mapNotNull(BadgeCatalog::findById)
        }

        return QuizAward(
            score = score,
            correctCount = correctCount,
            isNewBest = isNewBest,
            totalPoints = totalPoints.first(),
            newBadges = newBadges,
        )
    }

    private fun evaluateBadges(bestScores: Map<String, Int>): Set<String> {
        val earned = mutableSetOf<String>()
        val anyCompleted = bestScores.values.any { it > 0 }
        val kaleScore = bestScores["harput_kalesi"] ?: 0
        val sarnicScore = bestScores["urartu_sarnici_zindani"] ?: 0

        if (anyCompleted) earned += BadgeCatalog.KESIF_YOLCUSU.id
        if (kaleScore >= GUARDIAN_THRESHOLD) earned += BadgeCatalog.KALE_MUHAFIZI.id
        if (sarnicScore >= GUARDIAN_THRESHOLD) earned += BadgeCatalog.SARNIC_BEKCISI.id
        if (bestScores.values.any { it >= QuizRepository.MAX_SCORE }) {
            earned += BadgeCatalog.KUSURSUZ_HAFIZA.id
        }
        if (kaleScore >= SCHOLAR_THRESHOLD && sarnicScore >= SCHOLAR_THRESHOLD) {
            earned += BadgeCatalog.HARPUT_ALIMI.id
        }
        return earned
    }

    companion object {
        val TRACKED_LOCATION_IDS = listOf("harput_kalesi", "urartu_sarnici_zindani")
        const val GUARDIAN_THRESHOLD = 70
        const val SCHOLAR_THRESHOLD = 90
    }
}
