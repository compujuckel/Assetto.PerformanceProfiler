--if true then return end -- TODO

local config
local scenes

local scenesPath = ac.getFolder(ac.FolderID.ScriptOrigin) .. "\\scenes.json"
if io.fileExists(scenesPath) then
    config = JSON.parse(io.load(scenesPath))
    scenes = config.Scenes
    ac.debug("scenes", scenes)
    --io.deleteFile(scenesPath)
end

if scenes == nil then return end

local sim = ac.getSim()
local diagnostics = require("shared/utils/diagnostics")

local mmf = ac.writeMemoryMappedFile("Assetto.PerformanceProfiler.v1", [[
long counter;
double cpuTimeMs;
double gpuTimeMs;
double vramUsage;
int drawCalls;
int sceneTriangles;
int lights;
int extraShadows;
int scene;
char reset;
]])

local stats = {
    cpuTimeMs = 0,
    gpuTimeMs = 0,
    drawCalls = 0,
    sceneTriangles = 0,
    lights = 0,
    extraShadows = 0,
    vram = 0
}

local i = 0
local sceneId = 0
local function updateMMF(dt)
    mmf.counter = i
    mmf.cpuTimeMs = stats.cpuTimeMs
    mmf.gpuTimeMs = stats.gpuTimeMs
    mmf.drawCalls = stats.drawCalls
    mmf.sceneTriangles = stats.sceneTriangles
    mmf.lights = stats.lights
    mmf.extraShadows = stats.extraShadows
    mmf.vramUsage = stats.vram
    mmf.scene = sceneId

    i = i + 1
end

local clock_currentSceneStart
local clock_currentSceneEnd
local clock
local currentScene = 0
local waitTime = 5

local function reset()
    ac.log("reset")
    clock_currentSceneStart = os.preciseClock() + waitTime
    clock_currentSceneEnd = 0
    currentScene = 0
    i = 0
    sceneId = 0

    mmf.counter = 0
    mmf.scene = 0

    ac.tryToStart(true)
end

ui.onExclusiveHUD(function (mode)
    if mode ~= "game" then return end

    ui.toolWindow("PerformanceProfiler", vec2(10, 10), vec2(250, 200), function ()
        ui.text("Performance Profiler running...")
        ui.text("Runs")
        ui.offsetCursorY(-5)
        ui.progressBar(config.CurrentRun / config.TotalRuns, vec2(200, 15), string.format("%d / %d", config.CurrentRun, config.TotalRuns))
        ui.text("Scenes")
        ui.offsetCursorY(-5)
        ui.progressBar(currentScene / #scenes, vec2(200, 15), string.format("%d / %d", currentScene, #scenes))
        ui.text("Current Scene")
        ui.offsetCursorY(-5)
        ui.progressBar((clock - clock_currentSceneStart + waitTime) / (clock_currentSceneEnd - clock_currentSceneStart + waitTime), vec2(200, 15))
    end)
end)
reset()

local function updateStats()
    stats.cpuTimeMs, stats.gpuTimeMs = ac.getPerformanceCPUAndGPUTime()
    local renderStats = diagnostics.renderStats()
    stats.drawCalls = renderStats.drawCalls
    stats.sceneTriangles = renderStats.sceneTriangles
    stats.lights = renderStats.lights
    stats.extraShadows = renderStats.extraShadows
    stats.vram = ac.getVRAMConsumption().usage

    ac.debug("stats", stats)
end

local function applyScene(scene)
    if scene.CameraMode ~= nil then
        ac.setCurrentCamera(scene.CameraMode)
    end

    if scene.DrivableCamera ~= nil then
        ac.setCurrentDrivableCamera(scene.DrivableCamera)
    end

    if scene.CameraPosition ~= nil and scene.CameraLook ~= nil and scene.CameraUp ~= nil then
        ac.setCameraPosition(vec3(scene.CameraPosition.X, scene.CameraPosition.Y, scene.CameraPosition.Z))
        ac.setCameraDirection(vec3(scene.CameraLook.X, scene.CameraLook.Y, scene.CameraLook.Z), vec3(scene.CameraUp.X, scene.CameraUp.Y, scene.CameraUp.Z))
    end

    if scene.CameraFOV ~= nil then
        ac.setCameraFOV(scene.CameraFOV)
    end
end

function script.update(dt)
    if sim.isInMainMenu then
        ac.tryToStart(true)
    end

    clock = os.preciseClock()
    updateStats()

    ac.debug("i", i)
    ac.debug("currentScene", currentScene)
    ac.debug("mmf.reset", mmf.reset)
    ac.debug("clock", clock)
    ac.debug("clock_currentSceneStart", clock_currentSceneStart)
    ac.debug("clock_currentSceneEnd", clock_currentSceneEnd)
    ac.debug("fov", ac.getCameraFOV())

    if mmf.reset == 1 then
        reset()
        mmf.reset = 0
    end

    if currentScene == 0 or clock > clock_currentSceneEnd then
        ac.log("end current scene")
        currentScene = currentScene + 1

        if currentScene > #scenes then
            ac.log("starting over")
            currentScene = 1
        end

        sceneId = currentScene - 1

        local scene = scenes[currentScene]
        ac.debug("DurationSeconds", scene.DurationSeconds)
        clock_currentSceneStart = clock + waitTime
        clock_currentSceneEnd = clock_currentSceneStart + scene.DurationSeconds

        applyScene(scene)
    end

    if clock < clock_currentSceneStart then
        return
    end

    updateMMF(dt)
end
