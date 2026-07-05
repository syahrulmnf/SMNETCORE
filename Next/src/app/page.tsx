'use client';

import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Typography from '@mui/material/Typography';

export default function HomePage() {
  return (
    <main className="min-h-screen flex items-center justify-center bg-gray-100 p-10">
      <Card className="w-full max-w-md rounded-2xl shadow-xl">
        <CardContent className="space-y-4">
          <Typography variant="h4" fontWeight="bold">
            Next.js + MUI + Tailwind
          </Typography>

          <Typography color="text.secondary">
            Production-ready starter template
          </Typography>

          <Button variant="contained" fullWidth>
            Get Started
          </Button>
        </CardContent>
      </Card>
    </main>
  );
}