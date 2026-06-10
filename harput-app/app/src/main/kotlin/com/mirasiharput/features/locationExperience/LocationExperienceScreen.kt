package com.mirasiharput.features.locationExperience

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.ViewInAr
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.mirasiharput.navigation.AppExperienceState
import com.mirasiharput.ui.theme.BackButtonBlack
import com.mirasiharput.ui.theme.BackIconWhite
import com.mirasiharput.ui.theme.BackgroundBlack
import com.mirasiharput.ui.theme.HeritageGold
import com.mirasiharput.ui.theme.ParchmentLight

@Composable
fun LocationExperienceScreen(
    state: AppExperienceState.LocationExperience,
    onBack: () -> Unit,
    onAudioToggle: () -> Unit,
    onModelLoadFailed: () -> Unit,
    onOpenArExperience: () -> Unit,
    modifier: Modifier = Modifier,
) {
    Column(
        modifier = modifier
            .fillMaxSize()
            .background(BackgroundBlack),
    ) {
        Box(
            modifier = Modifier
                .fillMaxWidth()
                .weight(0.55f)
                .background(Color.White),
        ) {
            ARModelViewer(
                location = state.location,
                onModelLoadFailed = onModelLoadFailed,
            )

            IconButton(
                onClick = onBack,
                modifier = Modifier
                    .align(Alignment.TopStart)
                    .padding(16.dp)
                    .size(48.dp)
                    .background(BackButtonBlack, RoundedCornerShape(30.dp)),
            ) {
                Icon(
                    imageVector = Icons.AutoMirrored.Filled.ArrowBack,
                    contentDescription = "Geri",
                    tint = BackIconWhite,
                )
            }

            Button(
                onClick = onOpenArExperience,
                modifier = Modifier
                    .align(Alignment.BottomCenter)
                    .padding(bottom = 16.dp)
                    .height(48.dp),
                shape = RoundedCornerShape(24.dp),
                colors = ButtonDefaults.buttonColors(
                    containerColor = HeritageGold,
                    contentColor = ParchmentLight,
                ),
            ) {
                Icon(
                    imageVector = Icons.Default.ViewInAr,
                    contentDescription = null,
                    modifier = Modifier.size(22.dp),
                )
                Spacer(modifier = Modifier.width(8.dp))
                Text(
                    text = "AR Deneyimi",
                    fontFamily = FontFamily.Serif,
                    fontWeight = FontWeight.Bold,
                    fontSize = 16.sp,
                )
            }
        }

        LocationInfoPanel(
            description = state.location.description,
            isAudioActive = state.isAudioActive,
            onAudioToggle = onAudioToggle,
            modifier = Modifier
                .fillMaxWidth()
                .weight(0.45f),
        )
    }
}
