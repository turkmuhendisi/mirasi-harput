package com.mirasiharput.features.home

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.CheckCircle
import androidx.compose.material.icons.outlined.RadioButtonUnchecked
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.foundation.layout.FlowRow
import androidx.compose.foundation.layout.ExperimentalLayoutApi
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.background
import com.mirasiharput.R
import com.mirasiharput.data.LocationRepository
import com.mirasiharput.data.achievements.BadgeCatalog
import com.mirasiharput.ui.theme.HeritageGoldSoft
import com.mirasiharput.ui.components.HeritageBackground
import com.mirasiharput.ui.components.HeritagePrimaryButton
import com.mirasiharput.ui.components.HeritageSecondaryButton
import com.mirasiharput.ui.components.OrnamentDivider
import com.mirasiharput.ui.theme.CertificateGreen
import com.mirasiharput.ui.theme.HeritageBrown
import com.mirasiharput.ui.theme.HeritageBrownSoft
import com.mirasiharput.ui.theme.HeritageGold
import com.mirasiharput.ui.theme.ParchmentLight

@Composable
fun HomeScreen(
    visitedLocationIds: Set<String>,
    certificateEarned: Boolean,
    totalPoints: Int,
    earnedBadgeIds: Set<String>,
    onStartExploring: () -> Unit,
    onClaimCertificate: () -> Unit,
    modifier: Modifier = Modifier,
) {
    HeritageBackground(modifier = modifier) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .verticalScroll(rememberScrollState())
                .padding(horizontal = 28.dp, vertical = 40.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center,
        ) {
            Spacer(modifier = Modifier.height(8.dp))

            Text(
                text = "ELAZIĞ · HARPUT",
                fontFamily = FontFamily.Serif,
                fontWeight = FontWeight.Medium,
                fontSize = 13.sp,
                letterSpacing = 4.sp,
                color = HeritageGold,
            )

            Spacer(modifier = Modifier.height(12.dp))

            Text(
                text = "Miras-ı Harput",
                fontFamily = FontFamily.Serif,
                fontWeight = FontWeight.Bold,
                fontSize = 36.sp,
                color = HeritageBrown,
                textAlign = TextAlign.Center,
            )

            Spacer(modifier = Modifier.height(16.dp))
            OrnamentDivider()
            Spacer(modifier = Modifier.height(16.dp))

            Text(
                text = "Tarihin izinde, artırılmış gerçeklikle Harput'un binlerce yıllık mirasını keşfedin.",
                fontFamily = FontFamily.Serif,
                fontSize = 16.sp,
                lineHeight = 24.sp,
                color = HeritageBrownSoft,
                textAlign = TextAlign.Center,
                modifier = Modifier.padding(horizontal = 4.dp),
            )

            Spacer(modifier = Modifier.height(28.dp))

            ProgressCard(visitedLocationIds = visitedLocationIds)

            Spacer(modifier = Modifier.height(20.dp))

            if (totalPoints > 0 || earnedBadgeIds.isNotEmpty()) {
                AchievementsCard(
                    totalPoints = totalPoints,
                    earnedBadgeIds = earnedBadgeIds,
                )
                Spacer(modifier = Modifier.height(20.dp))
            }

            if (certificateEarned) {
                RewardCard(onClaimCertificate = onClaimCertificate)
                Spacer(modifier = Modifier.height(20.dp))
            }

            HeritagePrimaryButton(
                text = "Keşfetmeye Başla",
                onClick = onStartExploring,
            )

            Spacer(modifier = Modifier.height(20.dp))

            Text(
                text = "Mekanlara ait QR kodları okutarak deneyimi başlatın.",
                fontFamily = FontFamily.Serif,
                fontSize = 13.sp,
                color = HeritageBrownSoft,
                textAlign = TextAlign.Center,
            )
        }
    }
}

@Composable
private fun ProgressCard(visitedLocationIds: Set<String>) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(20.dp),
        colors = CardDefaults.cardColors(containerColor = ParchmentLight),
        border = BorderStroke(1.dp, HeritageGold),
    ) {
        Column(modifier = Modifier.padding(20.dp)) {
            Text(
                text = "Yolculuğunuz",
                fontFamily = FontFamily.Serif,
                fontWeight = FontWeight.Bold,
                fontSize = 18.sp,
                color = HeritageBrown,
            )
            Spacer(modifier = Modifier.height(12.dp))
            LocationRepository.getAll().forEach { location ->
                val visited = visitedLocationIds.contains(location.id)
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(vertical = 6.dp),
                    verticalAlignment = Alignment.CenterVertically,
                ) {
                    Icon(
                        imageVector = if (visited) {
                            Icons.Filled.CheckCircle
                        } else {
                            Icons.Outlined.RadioButtonUnchecked
                        },
                        contentDescription = null,
                        tint = if (visited) CertificateGreen else HeritageBrownSoft,
                        modifier = Modifier.size(22.dp),
                    )
                    Spacer(modifier = Modifier.size(12.dp))
                    Text(
                        text = location.title,
                        fontFamily = FontFamily.Serif,
                        fontSize = 16.sp,
                        fontWeight = if (visited) FontWeight.SemiBold else FontWeight.Normal,
                        color = HeritageBrown,
                    )
                }
            }
        }
    }
}

@OptIn(ExperimentalLayoutApi::class)
@Composable
private fun AchievementsCard(
    totalPoints: Int,
    earnedBadgeIds: Set<String>,
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(20.dp),
        colors = CardDefaults.cardColors(containerColor = ParchmentLight),
        border = BorderStroke(1.dp, HeritageGold),
    ) {
        Column(modifier = Modifier.padding(20.dp)) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically,
            ) {
                Text(
                    text = "Miras Puanı",
                    fontFamily = FontFamily.Serif,
                    fontWeight = FontWeight.Bold,
                    fontSize = 18.sp,
                    color = HeritageBrown,
                    modifier = Modifier.weight(1f),
                )
                Text(
                    text = "$totalPoints",
                    fontFamily = FontFamily.Serif,
                    fontWeight = FontWeight.Bold,
                    fontSize = 22.sp,
                    color = HeritageGold,
                )
            }

            if (earnedBadgeIds.isNotEmpty()) {
                Spacer(modifier = Modifier.height(12.dp))
                Text(
                    text = "Rozetler",
                    fontFamily = FontFamily.Serif,
                    fontWeight = FontWeight.SemiBold,
                    fontSize = 15.sp,
                    color = HeritageBrownSoft,
                )
                Spacer(modifier = Modifier.height(8.dp))
                FlowRow {
                    BadgeCatalog.all
                        .filter { it.id in earnedBadgeIds }
                        .forEach { badge ->
                            Row(
                                modifier = Modifier
                                    .padding(end = 8.dp, bottom = 8.dp)
                                    .background(
                                        HeritageGoldSoft.copy(alpha = 0.25f),
                                        RoundedCornerShape(20.dp),
                                    )
                                    .padding(horizontal = 12.dp, vertical = 6.dp),
                                verticalAlignment = Alignment.CenterVertically,
                            ) {
                                Box(
                                    modifier = Modifier
                                        .size(22.dp)
                                        .background(HeritageGoldSoft, CircleShape),
                                    contentAlignment = Alignment.Center,
                                ) {
                                    Text(text = badge.emblem, fontSize = 12.sp)
                                }
                                Spacer(modifier = Modifier.size(6.dp))
                                Text(
                                    text = badge.title,
                                    fontFamily = FontFamily.Serif,
                                    fontWeight = FontWeight.SemiBold,
                                    fontSize = 13.sp,
                                    color = HeritageBrown,
                                )
                            }
                        }
                }
            }
        }
    }
}

@Composable
private fun RewardCard(onClaimCertificate: () -> Unit) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(20.dp),
        colors = CardDefaults.cardColors(containerColor = ParchmentLight),
        border = BorderStroke(2.dp, HeritageGold),
    ) {
        Column(
            modifier = Modifier.padding(20.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
        ) {
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .height(140.dp)
                    .clip(RoundedCornerShape(12.dp)),
                contentAlignment = Alignment.Center,
            ) {
                Image(
                    painter = painterResource(id = R.drawable.certificate_preview),
                    contentDescription = "Sertifika önizleme",
                    modifier = Modifier.fillMaxSize(),
                )
            }

            Spacer(modifier = Modifier.height(16.dp))

            Text(
                text = "Tebrikler!",
                fontFamily = FontFamily.Serif,
                fontWeight = FontWeight.Bold,
                fontSize = 22.sp,
                color = HeritageGold,
            )
            Spacer(modifier = Modifier.height(6.dp))
            Text(
                text = "Tüm mekanları ziyaret ederek katılım sertifikanızı kazandınız.",
                fontFamily = FontFamily.Serif,
                fontSize = 15.sp,
                lineHeight = 22.sp,
                color = HeritageBrownSoft,
                textAlign = TextAlign.Center,
            )
            Spacer(modifier = Modifier.height(16.dp))
            HeritageSecondaryButton(
                text = "Sertifikamı Al",
                onClick = onClaimCertificate,
            )
        }
    }
}
