package com.mirasiharput.features.arExperience

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxHeight
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.navigationBarsPadding
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.layout.widthIn
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.rememberLazyListState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Close
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.mirasiharput.data.LocationModel
import com.mirasiharput.data.achievements.QuizAward
import com.mirasiharput.features.quiz.QuizRepository
import com.mirasiharput.ui.theme.CertificateGreen
import com.mirasiharput.ui.theme.HeritageBrown
import com.mirasiharput.ui.theme.HeritageBrownSoft
import com.mirasiharput.ui.theme.HeritageGold
import com.mirasiharput.ui.theme.HeritageGoldSoft
import com.mirasiharput.ui.theme.ParchmentBase
import com.mirasiharput.ui.theme.ParchmentDeep
import com.mirasiharput.ui.theme.ParchmentLight

private data class ChatEntry(val isBot: Boolean, val text: String)

/**
 * AR sahnesindeki modele dokunulduğunda açılan, mekan hakkında
 * 10 soruluk bilgi yarışması sunan sohbet kutusu.
 */
@Composable
fun ArQuizChatBox(
    location: LocationModel,
    quizAward: QuizAward?,
    onCompleteQuiz: (locationId: String, correctCount: Int) -> Unit,
    onClose: () -> Unit,
    modifier: Modifier = Modifier,
) {
    val questions = remember(location.id) { QuizRepository.getQuestions(location.id) }

    val messages = remember { mutableStateListOf<ChatEntry>() }
    var questionIndex by remember { mutableIntStateOf(0) }
    var correctCount by remember { mutableIntStateOf(0) }
    var awaitingNext by remember { mutableStateOf(false) }
    var finished by remember { mutableStateOf(false) }
    var resultReported by remember { mutableStateOf(false) }

    val listState = rememberLazyListState()

    LaunchedEffect(Unit) {
        messages += ChatEntry(
            isBot = true,
            text = "Merhaba, ben ${location.title} rehberinim. " +
                "Bu mekan hakkında ${questions.size} soruluk bilgi yarışmasına hoş geldin! " +
                "Her doğru cevap ${QuizRepository.POINTS_PER_CORRECT_ANSWER} puan kazandırır. Başlayalım!",
        )
        messages += ChatEntry(isBot = true, text = questions.first().text)
    }

    LaunchedEffect(messages.size, finished, quizAward) {
        listState.animateScrollToItem(maxOf(0, messages.size - 1))
    }

    LaunchedEffect(finished) {
        if (finished && !resultReported) {
            resultReported = true
            onCompleteQuiz(location.id, correctCount)
        }
    }

    Column(
        modifier = modifier
            .fillMaxHeight(0.62f)
            .background(
                ParchmentLight,
                RoundedCornerShape(topStart = 24.dp, topEnd = 24.dp),
            )
            .navigationBarsPadding(),
    ) {
        QuizHeader(
            title = "${location.title} · Bilgi Yarışması",
            progressText = if (finished) {
                "Tamamlandı"
            } else {
                "Soru ${minOf(questionIndex + 1, questions.size)}/${questions.size}"
            },
            onClose = onClose,
        )

        LazyColumn(
            state = listState,
            modifier = Modifier
                .weight(1f)
                .fillMaxWidth()
                .padding(horizontal = 16.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp),
        ) {
            items(messages) { entry ->
                ChatBubble(entry = entry)
            }
            if (finished) {
                item {
                    QuizResultCard(
                        correctCount = correctCount,
                        totalQuestions = questions.size,
                        quizAward = quizAward,
                    )
                }
            }
        }

        when {
            finished -> {
                Button(
                    onClick = onClose,
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(16.dp)
                        .height(52.dp),
                    shape = RoundedCornerShape(14.dp),
                    colors = ButtonDefaults.buttonColors(
                        containerColor = HeritageGold,
                        contentColor = ParchmentLight,
                    ),
                ) {
                    Text(
                        text = "Yarışmayı Kapat",
                        fontFamily = FontFamily.Serif,
                        fontWeight = FontWeight.Bold,
                        fontSize = 16.sp,
                    )
                }
            }

            awaitingNext -> {
                val isLast = questionIndex == questions.size - 1
                Button(
                    onClick = {
                        if (isLast) {
                            finished = true
                        } else {
                            questionIndex++
                            messages += ChatEntry(isBot = true, text = questions[questionIndex].text)
                            awaitingNext = false
                        }
                    },
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(16.dp)
                        .height(52.dp),
                    shape = RoundedCornerShape(14.dp),
                    colors = ButtonDefaults.buttonColors(
                        containerColor = HeritageBrown,
                        contentColor = ParchmentLight,
                    ),
                ) {
                    Text(
                        text = if (isLast) "Sonucu Gör" else "Sonraki Soru",
                        fontFamily = FontFamily.Serif,
                        fontWeight = FontWeight.Bold,
                        fontSize = 16.sp,
                    )
                }
            }

            else -> {
                val question = questions[questionIndex]
                Column(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(horizontal = 16.dp, vertical = 12.dp),
                    verticalArrangement = Arrangement.spacedBy(8.dp),
                ) {
                    question.options.forEachIndexed { index, option ->
                        OutlinedButton(
                            onClick = {
                                val isCorrect = index == question.correctIndex
                                if (isCorrect) correctCount++
                                messages += ChatEntry(isBot = false, text = option)
                                messages += ChatEntry(
                                    isBot = true,
                                    text = if (isCorrect) {
                                        "Doğru! ${question.explanation}"
                                    } else {
                                        "Yanlış. Doğru cevap: " +
                                            "${question.options[question.correctIndex]}. " +
                                            question.explanation
                                    },
                                )
                                awaitingNext = true
                            },
                            modifier = Modifier.fillMaxWidth(),
                            shape = RoundedCornerShape(12.dp),
                            border = BorderStroke(1.dp, HeritageGoldSoft),
                            colors = ButtonDefaults.outlinedButtonColors(
                                containerColor = ParchmentBase,
                                contentColor = HeritageBrown,
                            ),
                        ) {
                            Text(
                                text = option,
                                fontFamily = FontFamily.Serif,
                                fontSize = 14.sp,
                                textAlign = TextAlign.Center,
                            )
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun QuizHeader(
    title: String,
    progressText: String,
    onClose: () -> Unit,
) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(start = 20.dp, end = 8.dp, top = 12.dp, bottom = 8.dp),
        verticalAlignment = Alignment.CenterVertically,
    ) {
        Column(modifier = Modifier.weight(1f)) {
            Text(
                text = title,
                fontFamily = FontFamily.Serif,
                fontWeight = FontWeight.Bold,
                fontSize = 16.sp,
                color = HeritageBrown,
            )
            Text(
                text = progressText,
                fontFamily = FontFamily.Serif,
                fontSize = 13.sp,
                color = HeritageBrownSoft,
            )
        }
        IconButton(onClick = onClose) {
            Icon(
                imageVector = Icons.Default.Close,
                contentDescription = "Yarışmayı kapat",
                tint = HeritageBrown,
            )
        }
    }
}

@Composable
private fun ChatBubble(entry: ChatEntry) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = if (entry.isBot) Arrangement.Start else Arrangement.End,
    ) {
        Box(
            modifier = Modifier
                .widthIn(max = 300.dp)
                .background(
                    color = if (entry.isBot) ParchmentDeep else HeritageGold,
                    shape = RoundedCornerShape(
                        topStart = 16.dp,
                        topEnd = 16.dp,
                        bottomStart = if (entry.isBot) 4.dp else 16.dp,
                        bottomEnd = if (entry.isBot) 16.dp else 4.dp,
                    ),
                )
                .padding(horizontal = 14.dp, vertical = 10.dp),
        ) {
            Text(
                text = entry.text,
                fontFamily = FontFamily.Serif,
                fontSize = 14.sp,
                lineHeight = 20.sp,
                color = if (entry.isBot) HeritageBrown else ParchmentLight,
            )
        }
    }
}

@Composable
private fun QuizResultCard(
    correctCount: Int,
    totalQuestions: Int,
    quizAward: QuizAward?,
) {
    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 8.dp)
            .background(ParchmentBase, RoundedCornerShape(16.dp))
            .padding(16.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
    ) {
        Text(
            text = "Yarışma Tamamlandı!",
            fontFamily = FontFamily.Serif,
            fontWeight = FontWeight.Bold,
            fontSize = 18.sp,
            color = HeritageGold,
        )
        Spacer(modifier = Modifier.height(8.dp))
        Text(
            text = "$totalQuestions sorudan $correctCount doğru",
            fontFamily = FontFamily.Serif,
            fontSize = 15.sp,
            color = HeritageBrown,
        )

        if (quizAward == null) {
            Spacer(modifier = Modifier.height(8.dp))
            Text(
                text = "Puanınız hesaplanıyor…",
                fontFamily = FontFamily.Serif,
                fontSize = 13.sp,
                color = HeritageBrownSoft,
            )
            return@Column
        }

        Spacer(modifier = Modifier.height(8.dp))
        Text(
            text = "+${quizAward.score} Miras Puanı" +
                if (quizAward.isNewBest) " (yeni rekor!)" else "",
            fontFamily = FontFamily.Serif,
            fontWeight = FontWeight.Bold,
            fontSize = 16.sp,
            color = CertificateGreen,
        )
        Spacer(modifier = Modifier.height(4.dp))
        Text(
            text = "Toplam Miras Puanı: ${quizAward.totalPoints}",
            fontFamily = FontFamily.Serif,
            fontSize = 14.sp,
            color = HeritageBrownSoft,
        )

        if (quizAward.newBadges.isNotEmpty()) {
            Spacer(modifier = Modifier.height(12.dp))
            Text(
                text = "Yeni Rozetler",
                fontFamily = FontFamily.Serif,
                fontWeight = FontWeight.Bold,
                fontSize = 15.sp,
                color = HeritageBrown,
            )
            Spacer(modifier = Modifier.height(8.dp))
            quizAward.newBadges.forEach { badge ->
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(vertical = 4.dp)
                        .background(ParchmentLight, RoundedCornerShape(12.dp))
                        .padding(horizontal = 12.dp, vertical = 10.dp),
                    verticalAlignment = Alignment.CenterVertically,
                ) {
                    Box(
                        modifier = Modifier
                            .size(36.dp)
                            .background(HeritageGoldSoft, CircleShape),
                        contentAlignment = Alignment.Center,
                    ) {
                        Text(text = badge.emblem, fontSize = 18.sp)
                    }
                    Spacer(modifier = Modifier.width(10.dp))
                    Column {
                        Text(
                            text = badge.title,
                            fontFamily = FontFamily.Serif,
                            fontWeight = FontWeight.Bold,
                            fontSize = 14.sp,
                            color = HeritageBrown,
                        )
                        Text(
                            text = badge.description,
                            fontFamily = FontFamily.Serif,
                            fontSize = 12.sp,
                            lineHeight = 16.sp,
                            color = HeritageBrownSoft,
                        )
                    }
                }
            }
        }
    }
}
