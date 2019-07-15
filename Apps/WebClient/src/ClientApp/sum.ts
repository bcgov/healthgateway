export default function sum(f: number, s: number) {
  if (Number.isNaN(f)) {
    console.log("f is not a number");
  }

  if (Number.isNaN(s)) {
    console.log("s is not a number");
  }

  return f + s;
}

export function sub(f: number, s: number) {
  return f - s;
}