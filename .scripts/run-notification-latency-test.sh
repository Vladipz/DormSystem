#!/usr/bin/env bash

set -euo pipefail

API_URL="${API_URL:-http://localhost:5006/api/notifications/test/in-app}"
TITLE="${TITLE:-Latency test}"
MESSAGE_PREFIX="${MESSAGE_PREFIX:-Test notification}"
REQUESTS="${REQUESTS:-100}"

USER_ONE_ID="${USER_ONE_ID:-44444444-4444-4444-4444-444444444444}"
USER_TWO_ID="${USER_TWO_ID:-55555555-5555-5555-5555-555555555555}"

REQUEST_TIMES=()
START_TIME="$(date +%s)"

print_stats() {
  local samples count min max avg p50 p95 total_elapsed

  count="${#REQUEST_TIMES[@]}"
  total_elapsed="$(( $(date +%s) - START_TIME ))"

  printf '\nRun statistics\n'
  printf 'Total requests: %s\n' "$REQUESTS"
  printf 'Completed requests: %s\n' "$count"
  printf 'Total wall time: %ss\n' "$total_elapsed"

  if [ "$count" -eq 0 ] ; then
    printf 'No completed requests to summarize.\n'
    return
  fi

  samples="$(printf '%s\n' "${REQUEST_TIMES[@]}" | sort -n)"

  min="$(printf '%s\n' "$samples" | awk 'NR == 1 { print }')"
  max="$(printf '%s\n' "$samples" | awk 'END { print }')"
  avg="$(printf '%s\n' "$samples" | awk '{ sum += $1 } END { printf "%.3f", sum / NR }')"
  p50="$(printf '%s\n' "$samples" | awk '{ values[NR] = $1 } END { idx = int((NR - 1) * 0.50) + 1; print values[idx] }')"
  p95="$(printf '%s\n' "$samples" | awk '{ values[NR] = $1 } END { idx = int((NR - 1) * 0.95) + 1; print values[idx] }')"

  printf 'Request time min: %ss\n' "$min"
  printf 'Request time avg: %ss\n' "$avg"
  printf 'Request time p50: %ss\n' "$p50"
  printf 'Request time p95: %ss\n' "$p95"
  printf 'Request time max: %ss\n' "$max"
}

printf 'Running %s requests to %s\n' "$REQUESTS" "$API_URL"
printf 'Users: %s, %s\n' "$USER_ONE_ID" "$USER_TWO_ID"
printf 'Interval: random 1.0-10.0s\n'

for i in $(seq 1 "$REQUESTS"); do
  REQUEST_TIME="$(curl -sS -o /dev/null -w '%{time_total}' -X POST "$API_URL" \
    -H "Content-Type: application/json" \
    -d "{
      \"userIds\": [
        \"$USER_ONE_ID\",
        \"$USER_TWO_ID\"
      ],
      \"title\": \"$TITLE\",
      \"message\": \"$MESSAGE_PREFIX $i\",
      \"count\": 1
    }")"

  REQUEST_TIMES+=("$REQUEST_TIME")

  printf '[%03d/%03d] sent in %ss\n' "$i" "$REQUESTS" "$REQUEST_TIME"

  if [ "$i" -lt "$REQUESTS" ]; then
    SLEEP_SECONDS=$(awk 'BEGIN { srand(); printf "%.1f", 1 + rand() * 9 }')
    printf 'Sleeping %ss before next request\n' "$SLEEP_SECONDS"
    sleep "$SLEEP_SECONDS"
  fi
done

print_stats

printf 'Done. Check Aspire metric notifications.delivery_latency_ms.\n'
