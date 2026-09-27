#!/usr/bin/env bash
# Asserts that the checked-out branch is what fixtures.yaml says it is.
#
#   main              the full suite passes, and every fixture's regression test exists and
#                     passes - the positive control. A regression test that is green on main
#                     only because it no longer runs would make every fixture look planted.
#   fixture/<name>    exactly ONE test fails, and it is that fixture's regression test.
#                     A second red test means the branch broke something it was not meant to;
#                     zero means the bug is not there and the image would test nothing.
#
# Usage: scripts/check-fixture.sh [branch]      (default: $GITHUB_REF_NAME, else git's branch)
#
# On success prints `id=<fixture id>` (or `id=main`) as its last line, which the workflow
# appends to $GITHUB_OUTPUT. Needs dotnet, yq (mikefarah v4) and python3 - all three are on
# GitHub's ubuntu runners.
set -Eeuo pipefail

cd "$(dirname "$0")/.."

branch="${1:-${GITHUB_REF_NAME:-$(git rev-parse --abbrev-ref HEAD)}}"

results="$(mktemp -d)"
trap 'rm -rf "$results"' EXIT

# The whole suite, every time. Filtering to the planted test would prove it fails and say
# nothing about whether everything else still passes.
set +e
dotnet test Shop.sln --nologo --logger "trx;LogFileName=results.trx" --results-directory "$results" >&2
status=$?
set -e

trx="$results/results.trx"
[ -f "$trx" ] || { echo "no test results were written (dotnet test exited $status)" >&2; exit 1; }

# One line per test: <outcome>\t<fully qualified name>
outcomes=$(python3 - "$trx" <<'PY'
import sys, xml.etree.ElementTree as ET
ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
for r in ET.parse(sys.argv[1]).getroot().iterfind(".//t:UnitTestResult", ns):
    print(f"{r.get('outcome')}\t{r.get('testName')}")
PY
)

total=$(printf '%s\n' "$outcomes" | grep -c . || true)
failed=$(printf '%s\n' "$outcomes" | awk -F'\t' '$1 != "Passed"' | cut -f2)

[ "$total" -gt 0 ] || { echo "the suite ran zero tests" >&2; exit 1; }

planted_passed() {
    printf '%s\n' "$outcomes" | awk -F'\t' -v t="$1" '$1 == "Passed" && ($2 == t || $2 ~ ("\\." t "$"))' | grep -q .
}

case "$branch" in
    main)
        if [ -n "$failed" ]; then
            printf 'main must be green; failing:\n%s\n' "$failed" >&2
            exit 1
        fi

        for t in $(yq -r '.fixtures[].test' fixtures.yaml | sort -u); do
            planted_passed "$t" || { echo "main does not run $t, so no fixture can be proven planted" >&2; exit 1; }
        done

        echo "main: $total tests, all green; every fixture's regression test present and passing" >&2
        echo "id=main"
        ;;

    fixture/*)
        id=$(yq -r ".fixtures[] | select(.branch == \"$branch\") | .id" fixtures.yaml)
        test=$(yq -r ".fixtures[] | select(.branch == \"$branch\") | .test" fixtures.yaml)

        [ -n "$id" ] && [ "$id" != "null" ] \
            || { echo "$branch is not declared in fixtures.yaml" >&2; exit 1; }

        count=$(printf '%s' "$failed" | grep -c . || true)
        if [ "$count" -ne 1 ]; then
            printf '%s must fail exactly one test (%s); %s failed:\n%s\n' \
                "$branch" "$test" "$count" "$failed" >&2
            exit 1
        fi

        case "$failed" in
            "$test"|*".$test") ;;
            *) echo "$branch fails $failed, not its planted test $test" >&2; exit 1 ;;
        esac

        echo "$branch ($id): $total tests, exactly the planted $test is red" >&2
        echo "id=$id"
        ;;

    *)
        echo "$branch is neither main nor a fixture/** branch; nothing to assert" >&2
        exit 1
        ;;
esac
