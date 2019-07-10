export default function sum(f: number, s: number) {
  if (f === NaN) {
    console.log("f is not a number");
  }

  if (s === NaN) {
    console.log("s is not a number");
  }

  return f + s;
}

export function sub(f: number, s: number) {
  return f - s;
}