package com.mirasiharput.features.arExperience

import android.view.MotionEvent
import androidx.activity.ComponentActivity
import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.fadeIn
import androidx.compose.animation.fadeOut
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.statusBarsPadding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.QuestionAnswer
import androidx.compose.material.icons.filled.TouchApp
import androidx.compose.material.icons.outlined.Radar
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.key
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.viewinterop.AndroidView
import androidx.lifecycle.compose.LocalLifecycleOwner
import com.google.ar.core.Config
import com.google.ar.core.Frame
import com.google.ar.core.Pose
import com.google.ar.core.Plane
import com.google.ar.core.TrackingState
import kotlin.math.atan2
import com.mirasiharput.data.LocationModel
import com.mirasiharput.data.achievements.QuizAward
import com.mirasiharput.ui.theme.BackButtonBlack
import com.mirasiharput.ui.theme.BackIconWhite
import com.mirasiharput.ui.theme.BackgroundBlack
import com.mirasiharput.ui.theme.HeritageBrown
import com.mirasiharput.ui.theme.HeritageGold
import com.mirasiharput.ui.theme.ParchmentLight
import io.github.sceneview.ar.ARSceneView
import io.github.sceneview.ar.arcore.createAnchorOrNull
import io.github.sceneview.ar.arcore.getUpdatedPlanes
import io.github.sceneview.ar.node.AnchorNode
import io.github.sceneview.math.Position
import io.github.sceneview.math.Rotation
import io.github.sceneview.node.ModelNode
import io.github.sceneview.node.Node
import kotlinx.coroutines.delay

private const val MIN_SCAN_INFO_DURATION_MS = 4_000L

/**
 * AR oturumunu (resume) doğrudan composition frame'inde başlatmak ana iş parçacığını
 * kilitler ve ARCore "Failed to register sensor to queue" hatasıyla çöker. Bu gecikme,
 * ekran yerleşip pencere odağı alındıktan sonra oturumu başlatmak için kullanılır.
 */
private const val SESSION_RESUME_DELAY_MS = 600L

private enum class ArPhase { SCANNING, READY_TO_PLACE, PLACED }

@Composable
fun ARExperienceScreen(
    location: LocationModel,
    quizAward: QuizAward?,
    onCompleteQuiz: (locationId: String, correctCount: Int) -> Unit,
    onRestartQuiz: () -> Unit,
    onModelLoadFailed: () -> Unit,
    onSessionFailed: () -> Unit,
    onBack: () -> Unit,
    modifier: Modifier = Modifier,
) {
    val context = LocalContext.current
    val activity = context as? ComponentActivity
    val lifecycleOwner = LocalLifecycleOwner.current

    var readyToResume by remember { mutableStateOf(false) }
    var planeDetected by remember { mutableStateOf(false) }
    var minScanTimeElapsed by remember { mutableStateOf(false) }
    var modelPlaced by remember { mutableStateOf(false) }
    var quizVisible by remember { mutableStateOf(false) }
    var quizSession by remember { mutableIntStateOf(0) }
    var sessionFailed by remember { mutableStateOf(false) }
    val placedModelNode = remember { mutableStateOf<ModelNode?>(null) }
    val sceneViewRef = remember { mutableStateOf<ARSceneView?>(null) }

    LaunchedEffect(activity) {
        if (activity == null) {
            sessionFailed = true
            return@LaunchedEffect
        }
        delay(SESSION_RESUME_DELAY_MS)
        readyToResume = true
        delay(MIN_SCAN_INFO_DURATION_MS)
        minScanTimeElapsed = true
    }

    // Lifecycle'ı (resume tetikler) layout/draw geçişi dışında, kompozisyon
    // kapsamında bağla; başarısız olursa state ile zarifçe çıkış yap.
    LaunchedEffect(readyToResume, sceneViewRef.value) {
        val sceneView = sceneViewRef.value
        if (readyToResume && sceneView != null && sceneView.lifecycle == null) {
            runCatching { sceneView.lifecycle = lifecycleOwner.lifecycle }
                .onFailure { sessionFailed = true }
        }
    }

    // Oturum hatasını (her kaynaktan) tek noktada, güvenli şekilde işle.
    LaunchedEffect(sessionFailed) {
        if (sessionFailed) onSessionFailed()
    }

    val phase = when {
        modelPlaced -> ArPhase.PLACED
        planeDetected && minScanTimeElapsed -> ArPhase.READY_TO_PLACE
        else -> ArPhase.SCANNING
    }

    Box(
        modifier = modifier
            .fillMaxSize()
            .background(BackgroundBlack),
    ) {
        if (activity != null) {
            AndroidView(
                modifier = Modifier.fillMaxSize(),
                factory = { viewContext ->
                    // Lifecycle constructor'da VERİLMEZ. Verilseydi RESUMED durumda
                    // addObserver -> onResume -> session.resume() composition frame'inde
                    // senkron çalışır ve "Failed to register sensor to queue" ile çöker.
                    // Bunun yerine ekran yerleştikten sonra (update) lifecycle bağlanır.
                    ARSceneView(viewContext).apply {
                        sceneViewRef.value = this
                        planeRenderer.isEnabled = true

                        sessionConfiguration = { _, config ->
                            config.planeFindingMode = Config.PlaneFindingMode.HORIZONTAL
                            config.depthMode = Config.DepthMode.DISABLED
                            config.lightEstimationMode = Config.LightEstimationMode.ENVIRONMENTAL_HDR
                        }

                        this.onSessionFailed = { sessionFailed = true }

                        onSessionUpdated = { _, frame ->
                            if (!planeDetected) {
                                planeDetected = frame.getUpdatedPlanes()
                                    .any { it.trackingState == TrackingState.TRACKING }
                            }
                        }

                        setOnGestureListener(
                            onSingleTapConfirmed = { motionEvent, node ->
                                if (!modelPlaced) {
                                    if (planeDetected && minScanTimeElapsed) {
                                        val placed = tryPlaceModel(
                                            sceneView = this,
                                            motionEvent = motionEvent,
                                            location = location,
                                            onModelLoadFailed = onModelLoadFailed,
                                        )
                                        if (placed != null) {
                                            placedModelNode.value = placed
                                            modelPlaced = true
                                            planeRenderer.isEnabled = false
                                        }
                                    }
                                } else {
                                    val model = placedModelNode.value
                                    if (node != null && model != null && node.isDescendantOf(model)) {
                                        onRestartQuiz()
                                        quizSession++
                                        quizVisible = true
                                    }
                                }
                            },
                        )
                    }
                },
                onRelease = { sceneView ->
                    sceneViewRef.value = null
                    runCatching { sceneView.destroy() }
                },
            )
        }

        AnimatedVisibility(
            visible = !quizVisible,
            enter = fadeIn(),
            exit = fadeOut(),
            modifier = Modifier
                .align(Alignment.TopCenter)
                .statusBarsPadding()
                .padding(top = 72.dp, start = 20.dp, end = 20.dp),
        ) {
            ArInfoBanner(phase = phase)
        }

        IconButton(
            onClick = onBack,
            modifier = Modifier
                .align(Alignment.TopStart)
                .statusBarsPadding()
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

        if (quizVisible) {
            key(quizSession) {
                ArQuizChatBox(
                    location = location,
                    quizAward = quizAward,
                    onCompleteQuiz = onCompleteQuiz,
                    onClose = { quizVisible = false },
                    modifier = Modifier
                        .align(Alignment.BottomCenter)
                        .fillMaxWidth(),
                )
            }
        }
    }
}

private fun tryPlaceModel(
    sceneView: ARSceneView,
    motionEvent: MotionEvent,
    location: LocationModel,
    onModelLoadFailed: () -> Unit,
): ModelNode? {
    val frame = sceneView.frame ?: return null

    val hit = frame.hitTest(motionEvent.x, motionEvent.y).firstOrNull { hitResult ->
        val trackable = hitResult.trackable
        trackable is Plane &&
            trackable.type == Plane.Type.HORIZONTAL_UPWARD_FACING &&
            trackable.isPoseInPolygon(hitResult.hitPose)
    } ?: return null

    val anchor = hit.createAnchorOrNull() ?: return null

    val modelInstance = runCatching {
        sceneView.modelLoader.createModelInstance(assetFileLocation = location.modelPath)
    }.getOrNull()

    if (modelInstance == null) {
        anchor.detach()
        onModelLoadFailed()
        return null
    }

    val transform = location.modelTransform
    val faceCameraYaw = computeYawTowardCamera(frame, hit.hitPose)
    val anchorNode = AnchorNode(sceneView.engine, anchor)
    val modelNode = ModelNode(
        modelInstance = modelInstance,
        scaleToUnits = transform.scale,
        // Modelin tabanı zemine oturacak şekilde origin alt-orta noktaya alınır.
        centerOrigin = Position(x = 0.0f, y = -1.0f, z = 0.0f),
    ).apply {
        rotation = Rotation(
            x = transform.rotationX,
            y = transform.rotationY + faceCameraYaw,
            z = transform.rotationZ,
        )
    }

    anchorNode.addChildNode(modelNode)
    sceneView.addChildNode(anchorNode)
    return modelNode
}

/** Yerleştirme anında modelin ön yüzünü kameraya (kullanıcıya) çevirir. */
private fun computeYawTowardCamera(frame: Frame, anchorPose: Pose): Float {
    val cameraPose = frame.camera.pose
    val dx = cameraPose.tx() - anchorPose.tx()
    val dz = cameraPose.tz() - anchorPose.tz()
    return Math.toDegrees(atan2(dx.toDouble(), dz.toDouble())).toFloat()
}

private fun Node.isDescendantOf(target: Node): Boolean {
    var current: Node? = this
    while (current != null) {
        if (current == target) return true
        current = current.parent
    }
    return false
}

@Composable
private fun ArInfoBanner(phase: ArPhase, modifier: Modifier = Modifier) {
    val (icon, text) = when (phase) {
        ArPhase.SCANNING ->
            Icons.Outlined.Radar to
                "Zemin taranıyor… Telefonunuzu yavaşça hareket ettirerek çevrenizdeki zemini tarayın."

        ArPhase.READY_TO_PLACE ->
            Icons.Default.TouchApp to
                "Zemin algılandı! Modeli yerleştirmek için zeminde işaretli alana dokunun."

        ArPhase.PLACED ->
            Icons.Default.QuestionAnswer to
                "Model yerleştirildi! Mekan hakkındaki bilgi yarışmasını başlatmak için modele dokunun."
    }
    InfoBannerCard(icon = icon, text = text, modifier = modifier)
}

@Composable
private fun InfoBannerCard(
    icon: ImageVector,
    text: String,
    modifier: Modifier = Modifier,
) {
    Row(
        modifier = modifier
            .fillMaxWidth()
            .background(ParchmentLight.copy(alpha = 0.94f), RoundedCornerShape(16.dp))
            .padding(horizontal = 16.dp, vertical = 14.dp),
        verticalAlignment = Alignment.CenterVertically,
    ) {
        Icon(
            imageVector = icon,
            contentDescription = null,
            tint = HeritageGold,
            modifier = Modifier.size(26.dp),
        )
        Spacer(modifier = Modifier.width(12.dp))
        Text(
            text = text,
            fontFamily = FontFamily.Serif,
            fontWeight = FontWeight.SemiBold,
            fontSize = 14.sp,
            lineHeight = 20.sp,
            color = HeritageBrown,
        )
    }
}
