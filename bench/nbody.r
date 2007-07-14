REBOL [
	Title: "Simulate n-body problem"
	Author: {Krzysztof Kowalczyk (http://blog.kowalczyk.info)}
]

pi: 3.14159265358979323
solar_mass: 4 * pi * pi
days_per_year: 365.24

usage-and-exit: does [
  print "Usage: nbody.r <num>"
  halt
]

print join "args: " system/script/args

;if type? system/script/args = none! [usage-and-exit]

print join "len args: " to-string length? system/script/args

if (length? system/script/args) <> 1 [usage-and-exit]
n: to-integer system/script/args/1

sun: make object! [
  x: 0.0
  y: 0.0
  z: 0.0
  vx: 0.0
  vy: 0.0
  vz: 0.0
  mass: solar_mass
]

jupiter: make sun [
 x: 4.84143144246472090e+00
 y: -1.16032004402742839e+00
 z: -1.03622044471123109e-01
 vx: 1.66007664274403694e-03 * days_per_year
 vy: 7.69901118419740425e-03 * days_per_year
 vz: -6.90460016972063023e-05 * days_per_year
 mass: 9.54791938424326609e-04 * solar_mass
]

saturn: make sun [
 x: 8.34336671824457987e+00
 y: 4.12479856412430479e+00
 z: -4.03523417114321381e-01
 vx: -2.76742510726862411e-03 * days_per_year
 vy: 4.99852801234917238e-03 * days_per_year
 vz: 2.30417297573763929e-05 * days_per_year
 mass: 2.85885980666130812e-04 * solar_mass
]

uranus: make sun [
 x: 1.28943695621391310e+01
 y: -1.51111514016986312e+01
 z: -2.23307578892655734e-01
 vx: 2.96460137564761618e-03 * days_per_year
 vy: 2.37847173959480950e-03 * days_per_year
 vz: -2.96589568540237556e-05 * days_per_year
 mass: 4.36624404335156298e-05 * solar_mass
]

neptune: make sun [
 x: 1.53796971148509165e+01
 y: -2.59193146099879641e+01
 z: 1.79258772950371181e-01
 vx: 2.68067772490389322e-03 * days_per_year
 vy: 1.62824170038242295e-03 * days_per_year
 vz: -9.51592254519715870e-05 * days_per_year
 mass: 5.15138902046611451e-05 * solar_mass
]

offset_momentum: func [
  bodies
  /local px py pz b
] [
  px: 0.0
  py: 0.0
  pz: 0.0
  print mold b
  foreach b bodies [
    px: px + b/vx * b/mass
    py: py + b/vy * b/mass
    pz: pz + b/vz * b/mass
  ]
  sun/vx: - px / solar_mass
  sun/vy: - py / solar_mass
  sun/vz: - pz / solar_mass
]

bodies: [sun jupiter saturn uranus neptune]

offset_momentum bodies
