rows = [];

for i in range(26):
    
    chars = list(input().upper());
    line = "new char[] {'" + "','".join(chars) + "'}" + (',' if i != 25 else '');

    rows.append(line);

print("new char[][] {")
for row in rows:
    print(row);
print("};");