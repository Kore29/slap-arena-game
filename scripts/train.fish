#!/usr/bin/env fish

# Argument Handling
set RUN_ID $argv[1]
if test -z "$RUN_ID"
    set RUN_ID "SlapFinal_01"
end

echo "🚀 Iniciando entrenamiento ML-Agents con Run ID: $RUN_ID"
echo "--------------------------------------------------------"

# Activar el entorno virtual
if test -f "venv/bin/activate.fish"
    source venv/bin/activate.fish
else
    echo "❌ Error: No se encontró el entorno virtual en venv/"
    exit 1
end

# Ejecutar el entrenamiento
# Nota: mlagents-learn gestiona el Ctrl+C para guardar el modelo antes de salir
mlagents-learn Config/SlapArenaTrainer.yaml --run-id=$RUN_ID --force

# Post-Entrenamiento: Copiar el modelo ONNX automáticamente
echo "--------------------------------------------------------"
echo "📦 Entrenamiento finalizado. Procesando resultados..."

set MODEL_PATH "results/$RUN_ID/EnemyBehavior.onnx"

if test -f $MODEL_PATH
    mkdir -p Assets/ML-Agents/Models/
    cp $MODEL_PATH Assets/ML-Agents/Models/EnemyBehavior.onnx
    echo "✅ MODELO ACTUALIZADO: Assets/ML-Agents/Models/EnemyBehavior.onnx"
else
    echo "⚠ No se pudo encontrar el modelo exportado en: $MODEL_PATH"
    echo "Asegúrate de que el nombre del Behavior en el archivo YAML es 'EnemyBehavior'."
end
